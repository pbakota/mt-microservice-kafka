using CommonData.Dto;

using Microsoft.AspNetCore.Mvc;

using Orders.Models;
using Orders.Services;

namespace Orders.Controllers;

[ApiController]
[Route("/orders")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IOrdersService _service;

    public OrdersController(ILogger<OrdersController> logger, IOrdersService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CustomerOrder customerOrder)
    {
        _logger.LogInformation("Received: {}", customerOrder);
        return Ok(await _service.CreateOrderAsync(customerOrder));
    }
}
