namespace Payments.Models;

public record Payment
{
    public long Id { get; set; }
    public string Mode { get; set; } = null!;
    public long OrderId { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = null!;
}