namespace Stock.Models;

public record StockItem
{
    public long Id { get; set; }
    public string Item { get; set; } = null!;

    public int Quantity { get; set; }
}
