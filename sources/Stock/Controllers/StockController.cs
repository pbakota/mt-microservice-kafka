using CommonData.Helpers;

using Microsoft.AspNetCore.Mvc;

using Stock.Models;
using Stock.Services;

namespace Stock.Controllers;

[ApiController]
[Route("/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _service;

    public StockController(IStockService service)
        => _service = service;

    [HttpPost]
    public async Task<ActionResult<StockItem>> AddItem(StockItem stockItem)
        => Ok(await _service.AddItem(stockItem));

    [HttpGet]
    [Route("{name}")]
    public ActionResult<StockItem> GetItem([FromRoute] string name)
        => Ok(_service.GetItem(name));

    [HttpGet]
    public ActionResult<List<StockItem>> GetAll([FromQuery] Pagination pagination)
        => Ok(_service.GetAll(pagination));
}
