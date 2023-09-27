using System.Text.Json;

namespace CommonData.Dto;

public class DeliveryEvent
{
    public CustomerOrder Order { get; set; } = null!;
    public String Type { get; set; } = null!;
    public override string ToString() => JsonSerializer.Serialize(this);
}
