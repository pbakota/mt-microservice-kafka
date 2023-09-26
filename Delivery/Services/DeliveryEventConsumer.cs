using CommonData.Dto;

using Confluent.Kafka;

using Delivery.Models;

using MassTransit;

namespace Delivery.Services;

public class DeliveryEventConsumer : IConsumer<DeliveryEvent>
{
    private const string ReversedStockTopic = "csharp-reversed-stock";
    private readonly ILogger<DeliveryEventConsumer> _logger;
    private readonly DeliveryDbContext _dbContext;
    private readonly IProducer<Null, DeliveryEvent> _deliveryEventProducer;

    public DeliveryEventConsumer(ILogger<DeliveryEventConsumer> logger, DeliveryDbContext dbContext, 
        IProducer<Null, DeliveryEvent> deliveryEventProducer)
    {
        _logger = logger;
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

            await _deliveryEventProducer.ProduceAsync(ReversedStockTopic, new Message<Null, DeliveryEvent> { Value = reverseEvent });
            _logger.LogInformation("{} -> Event sent: {}", ReversedStockTopic, reverseEvent);
        }
    }
}