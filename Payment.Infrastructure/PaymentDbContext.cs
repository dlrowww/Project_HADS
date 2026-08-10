using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;

namespace Payment.Infrastructure
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        // 这里的集合名可以自己定，但要和实体名称一致或你要的表名一致
        public DbSet<PaymentRecord> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentRecord>(entity =>
            {
                entity.HasKey(p => p.PaymentId);         // 主键
                entity.Property(p => p.Status)
                      .HasConversion<string>();           // 枚举转字符串
                entity.HasIndex(p => p.BookingId).IsUnique();
                // 其他列配置留空即可
            });
        }
    }
}
