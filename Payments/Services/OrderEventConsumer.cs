using CommonData.Dto;

using Confluent.Kafka;

using MassTransit;

using Payments.Models;

namespace Payments.Services;

public class OrderEventConsumer : IConsumer<OrderEvent>
{
    private const string NewPaymentsTopic = "csharp-new-payments";
    private const string ReversedOrdersTopic = "csharp-reversed-orders";
    private readonly ILogger<OrderEventConsumer> _logger;
    private readonly PaymentDbContext _dbContext;
    private readonly IProducer<Null, PaymentEvent> _paymentEventProducer;
    private readonly IProducer<Null, OrderEvent> _orderEventProducer;

    public OrderEventConsumer(ILogger<OrderEventConsumer> logger, PaymentDbContext context,
        IProducer<Null, PaymentEvent> paymentEventProducer,
        IProducer<Null, OrderEvent> orderEventProducer)
    {
        _logger = logger;

        _dbContext = context;
        _paymentEventProducer = paymentEventProducer;
        _orderEventProducer = orderEventProducer;
    }

    public async Task Consume(ConsumeContext<OrderEvent> context)
    {
        _logger.LogInformation("Received: {}", context.Message);

        var order = context.Message.Order;

        var payment = new Payment
        {
            Amount = order.Amount,
            Mode = order.PaymentMethod,
            OrderId = order.OrderId,
            Status = "Success",
        };

        try
        {
            _dbContext.Add(payment);
            await _dbContext.SaveChangesAsync();

            var paymentEvent = new PaymentEvent
            {
                Order = order,
                Type = "PAYMENT_CREATED",
            };

            await _paymentEventProducer.ProduceAsync(NewPaymentsTopic,
                new Message<Null, PaymentEvent> { Value = paymentEvent });

            _logger.LogInformation("{} -> Event sent: {}", NewPaymentsTopic, paymentEvent);
        }
        catch (Exception)
        {
            payment.Status = "Failed";
            _dbContext.Update(payment);
            await _dbContext.SaveChangesAsync();

            var orderEvent = new OrderEvent
            {
                Order = order,
                Type = "ORDER_REVERSED",
            };

            await _orderEventProducer.ProduceAsync(ReversedOrdersTopic,
                new Message<Null, OrderEvent> { Value = orderEvent });

            _logger.LogInformation("{} -> Event sent: {}", ReversedOrdersTopic, orderEvent);
        }
    }
}