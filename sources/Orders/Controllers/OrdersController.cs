using CommonData.Dto;
using CommonData.Helpers;

using Microsoft.AspNetCore.Mvc;

using Orders.Models;
using Orders.Services;

namespace Orders.Controllers;

[ApiController]
[Route("/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _service;

    public OrdersController(IOrdersService service)
        => _service = service;

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CustomerOrder customerOrder)
        => Ok(await _service.CreateOrderAsync(customerOrder));

    [HttpGet]
    public async Task<ActionResult<Order>> GetOrders([FromQuery] Pagination pagination)
        => Ok(await _service.GetOrdersAsync(pagination));
}
