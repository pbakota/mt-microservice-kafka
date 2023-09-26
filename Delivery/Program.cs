using CommonData.Dto;
using CommonData.Helpers;

using Confluent.Kafka;

using Delivery.Models;
using Delivery.Services;

using MassTransit;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddSingleton(s => new ProducerBuilder<Null, DeliveryEvent>(new ProducerConfig { BootstrapServers = "playbox:9094" })
    .SetValueSerializer(new KafkaEventSerializer<DeliveryEvent>()).Build());

builder.Services.AddDbContext<DeliveryDbContext>(opt =>
    opt.UseSqlite(connectionString: "Data Source=db/deliveries.db")
);

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddConsumer<DeliveryEventConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host("playbox:9094");

            k.TopicEndpoint<Null, DeliveryEvent>("csharp-new-stock", "csharp-stock-group", c =>
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
