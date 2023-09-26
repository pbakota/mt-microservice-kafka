using CommonData.Configuration;
using CommonData.Dto;
using CommonData.Helpers;

using Confluent.Kafka;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Orders.Models;

namespace Orders.Services;

public interface IOrdersService
{
    Task<Order> CreateOrderAsync(CustomerOrder customerOrder);
    Task<List<Order>> GetOrdersAsync(Pagination pagination);
}

public class OrderService : IOrdersService
{
    private readonly ILogger<OrderService> _logger;
    private readonly KafkaConfiguration _kafkaConfiguration;
    private readonly OrderDbContext _dbContext;
    private readonly IProducer<Null, OrderEvent> _orderEventProducer;

    public OrderService(ILogger<OrderService> logger,
        IOptions<KafkaConfiguration> kafkaConfiguration,
        OrderDbContext context,
        IProducer<Null, OrderEvent> orderEventProducer)
    {
        _logger = logger;
        _kafkaConfiguration = kafkaConfiguration.Value;
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

            await _orderEventProducer.ProduceAsync(_kafkaConfiguration.Producers?["new-orders"].Topic, 
                new Message<Null, OrderEvent> { Value = orderEvent });

            _logger.LogInformation("{} -> Event sent: {}", _kafkaConfiguration.Producers?["new-orders"].Topic, orderEvent);

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

    public async Task<List<Order>> GetOrdersAsync(Pagination pagination)
        => await _dbContext.Orders.OrderByDescending(x => x.Id).Skip(pagination.Skip).Take(pagination.Top).ToListAsync();
}