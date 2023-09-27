using CommonData.Configuration;
using CommonData.Dto;
using CommonData.Helpers;

using Confluent.Kafka;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Orders.Models;
using Orders.Services;

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
builder.Services.AddScoped<IOrdersService, OrderService>();
builder.Services.AddSingleton(s => new ProducerBuilder<Null, OrderEvent>(new ProducerConfig {
    BootstrapServers = bootstrapServers
}).SetValueSerializer(new KafkaEventSerializer<OrderEvent>()).Build());
builder.Services.AddControllers();

builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseSqlite(connectionString: connectionString)
);

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddConsumer<ReverseOrderConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host(bootstrapServers);

            k.TopicEndpoint<Null, OrderEvent>(kafkaConfig.Consumers?["reversed-orders"].Topic, kafkaConfig.Consumers?["reversed-orders"].GroupId, c =>
            {
                c.SetValueDeserializer(new KafkaEventSerializer<OrderEvent>());
                c.AutoOffsetReset = AutoOffsetReset.Earliest;
                c.ConfigureConsumer<ReverseOrderConsumer>(context);
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
