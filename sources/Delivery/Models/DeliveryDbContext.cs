using Microsoft.EntityFrameworkCore;

namespace Delivery.Models;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
        : base(options)
    {
    }

    public DbSet<DeliveryItem> Deliveries { get; set; } = null!;
}
