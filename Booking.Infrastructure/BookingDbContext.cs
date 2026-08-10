using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Booking.Domain.Entities.Booking> Bookings { get; set; }
        public DbSet<DeparturePreferenceStat> DeparturePreferenceStats { get; set; }

        // ← **新增**：OfferChange 日志表
        public DbSet<OfferChange> OfferChanges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Booking 主键
            modelBuilder.Entity<Booking.Domain.Entities.Booking>()
                .HasKey(b => b.BookingId);

            // 2. BookingStatus 枚举映射成字符串
            modelBuilder.Entity<Booking.Domain.Entities.Booking>()
                .Property(b => b.Status)
                .HasConversion<string>();

            // 3. 偏好统计唯一索引
            modelBuilder.Entity<DeparturePreferenceStat>()
                .HasIndex(s => new { s.FromCity, s.ToCity })
                .IsUnique();

            // 4. OfferChange 索引
            modelBuilder.Entity<OfferChange>()
                .HasIndex(c => c.OfferId);
        }
    }
}
