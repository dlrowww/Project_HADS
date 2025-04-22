using Microsoft.EntityFrameworkCore;
using OfferInventory.Domain.Entities;

namespace OfferInventory.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TransportOffer> TransportOffers => Set<TransportOffer>();
}

