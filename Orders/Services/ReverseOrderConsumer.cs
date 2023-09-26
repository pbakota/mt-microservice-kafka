using CommonData.Dto;

using MassTransit;

using Orders.Models;

namespace Orders.Services;

public class ReverseOrderConsumer : IConsumer<OrderEvent>
{
    private readonly ILogger<ReverseOrderConsumer> _logger;
    private readonly OrderDbContext _dbContext;

    public ReverseOrderConsumer(ILogger<ReverseOrderConsumer> logger, OrderDbContext orderDbContext)
    {
        _logger = logger;
        _dbContext = orderDbContext;
    }
    public async Task Consume(ConsumeContext<OrderEvent> context)
    {
        _logger.LogInformation("Reverse order event: {}", context.Message);

        try
        {
            var orderEvent = context.Message;
            var order = _dbContext.Orders.Where(x => x.Id == orderEvent.Order.OrderId).FirstOrDefault();
            if (order != default)
            {
                order.Status = "Failed";

                _dbContext.Update(order);
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Order reversed");
        }
        catch (Exception)
        {
            _logger.LogError("Exception occured while reverting order details");
        }
    }
}