// Availability.Infrastructure/AvailabilityDbContext.cs
using Microsoft.EntityFrameworkCore;
using Availability.Domain.Enums;
using Availability.Domain.Entities;

namespace Availability.Infrastructure
{
    public class AvailabilityDbContext : DbContext
    {
        public AvailabilityDbContext(DbContextOptions<AvailabilityDbContext> options)
            : base(options) { }

        public DbSet<SeatLock> SeatLocks => Set<SeatLock>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<SeatLock>(e =>
            {
                e.ToTable("SeatLocks");
                e.HasKey(p => p.LockId);

                e.Property(p => p.NumberOfSeats)
                   .IsRequired();

                e.Property(p => p.Status)
                   .HasConversion<string>()          // ENUM → varchar
                   .IsRequired();

                // 可按需再加索引
                e.HasIndex(p => new { p.OfferId, p.Status });
                e.HasIndex(p => p.BookingId).IsUnique();
            });
        }
    }
}
