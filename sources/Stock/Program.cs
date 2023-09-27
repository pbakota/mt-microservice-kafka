using CommonData.Configuration;
using CommonData.Dto;
using CommonData.Helpers;

using Confluent.Kafka;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Stock.Models;
using Stock.Services;

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
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddSingleton(s => new ProducerBuilder<Null, DeliveryEvent>(new ProducerConfig
{
    BootstrapServers = bootstrapServers,
}).SetValueSerializer(new KafkaEventSerializer<DeliveryEvent>()).Build());

builder.Services.AddSingleton(s => new ProducerBuilder<Null, PaymentEvent>(new ProducerConfig
{
    BootstrapServers = bootstrapServers,
}).SetValueSerializer(new KafkaEventSerializer<PaymentEvent>()).Build());

builder.Services.AddDbContext<StockDbContext>(opt =>
    opt.UseSqlite(connectionString: connectionString)
);

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddConsumer<PaymentEventConsumer>();
        rider.AddConsumer<ReverseStockConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(bootstrapServers);

            k.TopicEndpoint<Null, PaymentEvent>(kafkaConfig.Consumers?["new-payments"].Topic, kafkaConfig.Consumers?["new-payments"].GroupId, c =>
            {
                c.SetValueDeserializer(new KafkaEventSerializer<PaymentEvent>());
                c.AutoOffsetReset = AutoOffsetReset.Earliest;
                c.ConfigureConsumer<PaymentEventConsumer>(context);
                c.CheckpointInterval = TimeSpan.FromSeconds(10);

                c.CreateIfMissing(m => m.NumPartitions = 2);
            });

            k.TopicEndpoint<Null, DeliveryEvent>(kafkaConfig.Consumers?["reversed-stock"].Topic, kafkaConfig.Consumers?["reversed-stock"].GroupId, c =>
            {
                c.SetValueDeserializer(new KafkaEventSerializer<DeliveryEvent>());
                c.AutoOffsetReset = AutoOffsetReset.Earliest;
                c.ConfigureConsumer<ReverseStockConsumer>(context);

                c.CreateIfMissing(m => m.NumPartitions = 2);
            });
        });
    });
});

builder.Services.AddControllers();
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
