using System.Text.Json;

namespace CommonData.Dto;

public record CustomerOrder {
    public string Item  { get; set; } = null!;
    public int Quantity  { get; set; }
    public double Amount { get; set; }
    public string PaymentMethod  { get; set; } = null!;
    public long OrderId  { get; set; }
    public string Address  { get; set; } = null!;
    public override string ToString() => JsonSerializer.Serialize(this);
}