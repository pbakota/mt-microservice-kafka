using CommonData.Configuration;
using CommonData.Dto;

using Confluent.Kafka;

using MassTransit;

using Microsoft.Extensions.Options;

using Stock.Models;

namespace Stock.Services;

public class PaymentEventConsumer : IConsumer<PaymentEvent>
{
    private readonly ILogger<PaymentEventConsumer> _logger;
    private readonly KafkaConfiguration _kafkaConfiguration;
    private readonly StockDbContext _dbContext;
    private readonly IProducer<Null, DeliveryEvent> _deliveryEventProducer;
    private readonly IProducer<Null, PaymentEvent> _paymentEventProducer;

    public PaymentEventConsumer(ILogger<PaymentEventConsumer> logger,
        IOptions<KafkaConfiguration> kafkaConfiguration,
        StockDbContext dbContext,
        IProducer<Null, DeliveryEvent> deliveryEventProducer,
        IProducer<Null, PaymentEvent> paymentEventProducer)
    {
        _logger = logger;
        _kafkaConfiguration = kafkaConfiguration.Value;
        _dbContext = dbContext;
        _deliveryEventProducer = deliveryEventProducer;
        _paymentEventProducer = paymentEventProducer;
    }

    public async Task Consume(ConsumeContext<PaymentEvent> context)
    {
        _logger.LogInformation("Received: {}", context.Message);

        var order = context.Message.Order;

        try
        {
            var inv = _dbContext.StockItems.Where(x => x.Item == order.Item).FirstOrDefault();
            if (inv == default)
            {
                _logger.LogWarning("Stock not exits so reverting the order");
                throw new Exception("Stock not available");
            }

            inv.Quantity += -order.Quantity;
            _dbContext.Update(inv);

            await _dbContext.SaveChangesAsync();

            var deliveryEvent = new DeliveryEvent
            {
                Order = order,
                Type = "STOCK_UPDATED",
            };

            await _deliveryEventProducer.ProduceAsync(_kafkaConfiguration.Producers?["new-stock"].Topic,
                new Message<Null, DeliveryEvent> { Value = deliveryEvent });

            _logger.LogInformation("{} -> Event sent: {}", _kafkaConfiguration.Producers?["new-stock"].Topic, deliveryEvent);
        }
        catch (Exception)
        {
            var paymentEvent = new PaymentEvent
            {
                Order = order,
                Type = "PAYMENT_REVERSED",
            };

            await _paymentEventProducer.ProduceAsync(_kafkaConfiguration.Producers?["reversed-payments"].Topic,
                new Message<Null, PaymentEvent> { Value = paymentEvent });
 
            _logger.LogInformation("{} -> Event sent: {}", _kafkaConfiguration.Producers?["reversed-payments"].Topic, paymentEvent);
        }
    }
}