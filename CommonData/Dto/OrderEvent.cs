using System.Text.Json;

namespace CommonData.Dto;

public record OrderEvent
{
    public CustomerOrder Order { get; set; } = null!;
    public string Type { get; set; } = null!;
    public override string ToString() => JsonSerializer.Serialize(this);
}