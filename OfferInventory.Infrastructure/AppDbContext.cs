using Microsoft.EntityFrameworkCore;
using OfferInventory.Domain.Entities;

namespace OfferInventory.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<TransportOffer> TransportOffers { get; set; }
        public DbSet<Seat> Seats { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransportOffer>()
                .HasMany(o => o.Seats)
                .WithOne()
                .HasForeignKey(s => s.TransportOfferId);
        }
    }
}

