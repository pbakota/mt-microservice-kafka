using Microsoft.AspNetCore.Mvc;

namespace CommonData.Helpers;

public record Pagination
{
    [FromQuery(Name = "top")]
    public int Top { get; set; } = 100;

    [FromQuery(Name = "skip")]
    public int Skip { get; set; } = 0;
}