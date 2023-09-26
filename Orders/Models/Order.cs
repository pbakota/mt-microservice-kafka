namespace Orders.Models;

public record Order
{
    public long Id { get; set; }

    public string Item { get; set; } = null!;

    public int Quantity { get; set; }

    public double Amount { get; set; }

    public string Status { get; set; } = null!;
}