using Microsoft.AspNetCore.Mvc;

using Stock.Models;
using Stock.Services;

namespace Stock.Controllers;

[ApiController]
[Route("/")]
public class StockController : ControllerBase
{
    private readonly ILogger<StockController> _logger;
    private readonly IStockService _service;

    public StockController(ILogger<StockController> logger, IStockService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    [Route("/addItem")]
    public async Task<ActionResult<StockItem>> AddItem(StockItem stockItem)
    {
        _logger.LogInformation("Received: {}", stockItem);
        return Ok(await _service.AddItem(stockItem));
    }

    [HttpGet]
    [Route("/getItem/{name}")]
    public ActionResult<StockItem> GetItem([FromRoute] string name) {
        return Ok(_service.GetItem(name));
    }
}
