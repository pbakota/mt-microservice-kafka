using CommonData.Dto;
using CommonData.Helpers;

using Confluent.Kafka;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Orders.Models;
using Orders.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IOrdersService, OrderService>();
builder.Services.AddSingleton(s => new ProducerBuilder<Null, OrderEvent>(new ProducerConfig { BootstrapServers = "playbox:9094" })
    .SetValueSerializer(new KafkaEventSerializer<OrderEvent>()).Build());
builder.Services.AddControllers();

builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseSqlite(connectionString: "Data Source=db/orders.db")
);

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(rider =>
    {
        rider.AddConsumer<ReverseOrderConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host("playbox:9094");

            k.TopicEndpoint<Null, OrderEvent>("csharp-reversed-orders", "csharp-orders-group", c =>
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
