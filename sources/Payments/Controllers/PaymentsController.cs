using CommonData.Helpers;

using Microsoft.AspNetCore.Mvc;

using Payments.Models;

namespace Orders.Controllers;

[ApiController]
[Route("/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _dbContext;

    public PaymentsController(PaymentDbContext dbContext)
        => _dbContext = dbContext;

    [HttpGet]
    public ActionResult<List<Payment>> GetPayments([FromQuery] Pagination pagination)
        => _dbContext.Payments.OrderByDescending(x => x.Id).Skip(pagination.Skip).Take(pagination.Top).ToList();
}
