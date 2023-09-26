using CommonData.Configuration;
using CommonData.Dto;

using Confluent.Kafka;

using MassTransit;

using Microsoft.Extensions.Options;

using Stock.Models;

namespace Stock.Services;

public class ReverseStockConsumer : IConsumer<DeliveryEvent>
{
    private readonly ILogger<ReverseStockConsumer> _logger;
    private readonly KafkaConfiguration _kafkaConfiguration;
    private readonly StockDbContext _dbContext;
    private readonly IProducer<Null, PaymentEvent> _paymentEventProducer;

    public ReverseStockConsumer(ILogger<ReverseStockConsumer> logger,
        IOptions<KafkaConfiguration> kafkaConfiguration,
        StockDbContext dbContext,
        IProducer<Null, PaymentEvent> paymentEventProducer)
    {
        _logger = logger;
        _kafkaConfiguration = kafkaConfiguration.Value;
        _dbContext = dbContext;
        _paymentEventProducer = paymentEventProducer;
    }

    public async Task Consume(ConsumeContext<DeliveryEvent> context)
    {
        _logger.LogInformation("Received: {}", context.Message);

        try
        {
            var order = context.Message.Order;

            var inv = _dbContext.StockItems.Where(x => x.Item == order.Item).ToList();
            inv.ForEach(async x =>
            {
                x.Quantity += order.Quantity;
                _dbContext.Update(x);

                await _dbContext.SaveChangesAsync();
            });

            var paymentEvent = new PaymentEvent
            {
                Order = order,
                Type = "PAYMENT_REVERSED",
            };

            await _paymentEventProducer.ProduceAsync(_kafkaConfiguration.Producers?["reversed-payments"].Topic,
                new Message<Null, PaymentEvent> { Value = paymentEvent });

            _logger.LogInformation("{} -> Event sent: {}", _kafkaConfiguration.Producers?["reversed-payments"].Topic, paymentEvent);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when reversing payment");
        }
    }
}