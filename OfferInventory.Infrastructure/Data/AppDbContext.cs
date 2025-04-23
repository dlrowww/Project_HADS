using Microsoft.EntityFrameworkCore;
using OfferInventory.Domain.Entities;

namespace OfferInventory.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<TransportOffer> TransportOffers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }


protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<TransportOffer>(b =>
    {
        b.Property(t => t.PriceTotal)
         .HasColumnType("decimal(8,2)");

        b.Property(t => t.PriceAverage)
         .HasColumnType("decimal(8,2)");

        b.Property(t => t.PriceOriginal)
         .HasColumnType("decimal(8,2)");
    });

    // ……如果还有其他字段需要精度限制，也在这里一并配置。
}
}

