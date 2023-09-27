using CommonData.Configuration;
using CommonData.Dto;

using Confluent.Kafka;

using Delivery.Models;

using MassTransit;

using Microsoft.Extensions.Options;

namespace Delivery.Services;

public class DeliveryEventConsumer : IConsumer<DeliveryEvent>
{
    private readonly ILogger<DeliveryEventConsumer> _logger;
    private readonly KafkaConfiguration _kafkaConfiguration;
    private readonly DeliveryDbContext _dbContext;
    private readonly IProducer<Null, DeliveryEvent> _deliveryEventProducer;

    public DeliveryEventConsumer(ILogger<DeliveryEventConsumer> logger,
        IOptions<KafkaConfiguration> kafkaConfiguration,
        DeliveryDbContext dbContext,
        IProducer<Null, DeliveryEvent> deliveryEventProducer)
    {
        _logger = logger;
        _kafkaConfiguration = kafkaConfiguration.Value;
        _dbContext = dbContext;
        _deliveryEventProducer = deliveryEventProducer;
    }

    public async Task Consume(ConsumeContext<DeliveryEvent> context)
    {
        _logger.LogInformation("Received: {}", context.Message);

        var deliveryEvent = context.Message;
        var order = deliveryEvent.Order;

        var shipment = new DeliveryItem
        {
            Address = order.Address,
            OrderId = order.OrderId,
            Status = "Success"
        };

        try
        {
            if (string.IsNullOrEmpty(order.Address))
            {
                throw new Exception("Address not present");
            }

            _dbContext.Add(shipment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Delivered to {}", shipment);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occured in delivery");

            shipment.OrderId = order.OrderId;
            shipment.Status = "Failed";

            _dbContext.Update(shipment);
            await _dbContext.SaveChangesAsync();

            var reverseEvent = new DeliveryEvent
            {
                Type = "STOCK_REVERSED",
                Order = order,
            };

            await _deliveryEventProducer.ProduceAsync(_kafkaConfiguration.Producers?["reversed-stock"].Topic,
                new Message<Null, DeliveryEvent> { Value = reverseEvent });

            _logger.LogInformation("{} -> Event sent: {}", _kafkaConfiguration.Producers?["reversed-stock"].Topic, reverseEvent);
        }
    }
}