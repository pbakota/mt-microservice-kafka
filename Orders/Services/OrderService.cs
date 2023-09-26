using CommonData.Dto;

using Confluent.Kafka;

using Orders.Models;

namespace Orders.Services;

public interface IOrdersService
{
    Task<Order> CreateOrderAsync(CustomerOrder customerOrder);
}

public class OrderService : IOrdersService
{
    private const string NewOrdersTopic = "csharp-new-orders";
    private readonly ILogger<OrderService> _logger;
    private readonly OrderDbContext _dbContext;
    private readonly IProducer<Null, OrderEvent> _orderEventProducer;

    public OrderService(ILogger<OrderService> logger, OrderDbContext context, IProducer<Null, OrderEvent> orderEventProducer)
    {
        _logger = logger;
        _dbContext = context;
        _orderEventProducer = orderEventProducer;
    }

    public async Task<Order> CreateOrderAsync(CustomerOrder customerOrder)
    {
        var order = new Order
        {
            Item = customerOrder.Item,
            Amount = customerOrder.Amount,
            Quantity = customerOrder.Quantity,
            Status = "Created",
        };

        try
        {
            var newOrder = await _dbContext.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            customerOrder.OrderId = newOrder.Entity.Id;

            var orderEvent = new OrderEvent
            {
                Order = customerOrder,
                Type = "ORDER_CREATED",
            };

            await _orderEventProducer.ProduceAsync(NewOrdersTopic, 
                new Message<Null, OrderEvent> { Value = orderEvent });

            _logger.LogInformation("{} -> Event sent: {}", NewOrdersTopic, orderEvent);

            return newOrder.Entity;
        }
        catch (Exception e)
        {
            _logger.LogError("Error occured in order", e);
            order.Status = "Failed";
            _dbContext.Update(order);

            await _dbContext.SaveChangesAsync();

            throw;
        }
    }
}