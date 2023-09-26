using CommonData.Helpers;

using Delivery.Models;

using Microsoft.AspNetCore.Mvc;

namespace Delivery.Controllers;

[ApiController]
[Route("/deliveries")]
public class DeliveriesController : ControllerBase
{
    private readonly DeliveryDbContext _dbContext;

    public DeliveriesController(DeliveryDbContext dbContext)
        => _dbContext = dbContext;

    [HttpGet]
    public ActionResult<List<DeliveryItem>> GetDeliveries([FromQuery] Pagination pagination)
        => Ok(_dbContext.Deliveries.OrderByDescending(x => x.Id).Skip(pagination.Skip).Take(pagination.Top).ToList());
}
