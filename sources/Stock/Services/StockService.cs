using System.Data.Entity;

using CommonData.Helpers;

using Stock.Models;

namespace Stock.Services;

public interface IStockService
{
    Task<StockItem> AddItem(StockItem item);
    StockItem? GetItem(string name);
    List<StockItem> GetAll(Pagination pagination);
}

// NOTE: Unfortunately sqlite3 provider does not support async query operations, therefore the queries are executed synchronously
public class StockService : IStockService
{
    private readonly StockDbContext _dbContext;

    public StockService(StockDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<StockItem> AddItem(StockItem item)
    {
        StockItem newStockItem;

        var old = _dbContext.StockItems.Where(x => x.Item == item.Item).FirstOrDefault();
        if (old != default)
        {
            old.Quantity += item.Quantity;
            _dbContext.Update(old);

            newStockItem = old;
        }
        else
        {
            var stockItem = new StockItem
            {
                Item = item.Item,
                Quantity = item.Quantity,
            };

            newStockItem = _dbContext.Add(stockItem).Entity;
        }

        await _dbContext.SaveChangesAsync();

        return newStockItem;
    }

    public List<StockItem> GetAll(Pagination pagination)
        => _dbContext.StockItems.OrderByDescending(x => x.Id).Skip(pagination.Skip).Take(pagination.Top).ToList();

    public StockItem? GetItem(string name)
        => _dbContext.StockItems.Where(x => x.Item == name).FirstOrDefault();
}
