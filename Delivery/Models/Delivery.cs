namespace Delivery.Models;

public record DeliveryItem
{
    public long Id { get; set; }
    public string Address { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long OrderId { get; set; }
}