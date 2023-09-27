using CommonData.Configuration;
using CommonData.Dto;
using CommonData.Helpers;

using Confluent.Kafka;

using Delivery.Models;
using Delivery.Services;

using MassTransit;

using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddSingleton(s => new ProducerBuilder<Null, DeliveryEvent>(new ProducerConfig
{
    BootstrapServers = bootstrapServers
}).SetValueSerializer(new KafkaEventSerializer<DeliveryEvent>()).Build());

builder.Services.AddDbContext<DeliveryDbContext>(opt =>
    opt.UseSqlite(connectionString: connectionString)
);

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddConsumer<DeliveryEventConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(bootstrapServers);

            k.TopicEndpoint<Null, DeliveryEvent>(kafkaConfig.Consumers?["new-stock"].Topic, kafkaConfig.Consumers?["new-stock"].GroupId, c =>
            {
                c.SetValueDeserializer(new KafkaEventSerializer<DeliveryEvent>());
                c.AutoOffsetReset = AutoOffsetReset.Earliest;
                c.ConfigureConsumer<DeliveryEventConsumer>(context);
                c.CheckpointInterval = TimeSpan.FromSeconds(10);

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
