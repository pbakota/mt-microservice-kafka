using Microsoft.AspNetCore.Mvc;

using Payments.Models;

namespace Orders.Controllers;

[ApiController]
[Route("/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ILogger<PaymentsController> _logger;
    private readonly PaymentDbContext _dbContext;

    public PaymentsController(ILogger<PaymentsController> logger, PaymentDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    public ActionResult<List<Payment>> GetPayments(){
        return _dbContext.Payments.ToList();
    }
}
