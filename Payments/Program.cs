using CommonData.Configuration;
using CommonData.Dto;
using CommonData.Helpers;

using Confluent.Kafka;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Payments.Models;
using Payments.Services;

var builder = WebApplication.CreateBuilder(args);

var kafkaConfig = new KafkaConfiguration();
var kafkaSection = builder.Configuration.GetSection("Kafka");

// Bind to local instance
kafkaSection.Bind(kafkaConfig);

// Make it available for other services
builder.Services.Configure<KafkaConfiguration>(kafkaSection);

var bootstrapServers = kafkaConfig.BootstrapServers;

var connectionString = builder.Configuration.GetConnectionString("sqlite")!;

// Add services to the container.
builder.Services.AddSingleton(s => new ProducerBuilder<Null, PaymentEvent>(new ProducerConfig
{
    BootstrapServers = bootstrapServers,
}).SetValueSerializer(new KafkaEventSerializer<PaymentEvent>()).Build());

builder.Services.AddSingleton(s => new ProducerBuilder<Null, OrderEvent>(new ProducerConfig
{
    BootstrapServers = bootstrapServers,
}).SetValueSerializer(new KafkaEventSerializer<OrderEvent>()).Build());

builder.Services.AddControllers();

builder.Services.AddDbContext<PaymentDbContext>(opt =>
    opt.UseSqlite(connectionString: connectionString)
);

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddConsumer<ReversePaymentConsumer>();
        rider.AddConsumer<OrderEventConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(bootstrapServers);

            k.TopicEndpoint<Null, PaymentEvent>(kafkaConfig.Consumers?["reversed-payments"].Topic, kafkaConfig.Consumers?["reversed-payments"].GroupId, c =>
            {
                c.SetValueDeserializer(new KafkaEventSerializer<PaymentEvent>());
                c.AutoOffsetReset = AutoOffsetReset.Earliest;
                c.ConfigureConsumer<ReversePaymentConsumer>(context);

                c.CreateIfMissing(m => m.NumPartitions = 2);
            });

            k.TopicEndpoint<Null, OrderEvent>(kafkaConfig.Consumers?["new-orders"].Topic, kafkaConfig.Consumers?["new-orders"].GroupId, c =>
            {
                c.SetValueDeserializer(new KafkaEventSerializer<OrderEvent>());
                c.AutoOffsetReset = AutoOffsetReset.Earliest;
                c.ConfigureConsumer<OrderEventConsumer>(context);
                c.CheckpointInterval = TimeSpan.FromSeconds(10);

                c.CreateIfMissing(m => m.NumPartitions = 2);
            });
        });
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// app.UseAuthorization();

app.MapControllers();

app.Run();
