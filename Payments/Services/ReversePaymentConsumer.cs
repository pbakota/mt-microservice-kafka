using CommonData.Dto;

using Confluent.Kafka;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Payments.Models;

namespace Payments.Services;

public class ReversePaymentConsumer : IConsumer<PaymentEvent>
{
    private const string ReversedOrdersTopic = "csharp-reversed-orders";
    private readonly ILogger<ReversePaymentConsumer> _logger;
    private readonly PaymentDbContext _context;
    private readonly IProducer<Null, OrderEvent> _orderProducer;

    public ReversePaymentConsumer(ILogger<ReversePaymentConsumer> logger, PaymentDbContext context, IProducer<Null, OrderEvent> orderProducer)
    {
        _logger = logger;
        _context = context;
        _orderProducer = orderProducer;
    }

    public async Task Consume(ConsumeContext<PaymentEvent> context)
    {
        _logger.LogInformation("Received: {}", context.Message);

        try
        {
            var order = context.Message.Order;

            var payments = await _context.Payments.Where(p => p.OrderId == order.OrderId).ToListAsync();
            payments.ForEach(p =>
            {
                p.Status = "Failed";
                _context.Update(p);
            });

            await _context.SaveChangesAsync();

            var orderEvent = new OrderEvent
            {
                Order = order,
                Type = "ORDER_REVERSED",
            };

            await _orderProducer.ProduceAsync(ReversedOrdersTopic, new Message<Null, OrderEvent> { Value = orderEvent });
            _logger.LogInformation("{} -> Event sent: {}", ReversedOrdersTopic, orderEvent);
        }
        catch (Exception)
        {
            _logger.LogError("Exception occured while reverting payment details");
            throw;
        }
    }
}