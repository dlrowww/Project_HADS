using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Payment.Domain.Entities;

namespace Payment.Infrastructure
{
    public class PaymentDesignTimeFactory : IDesignTimeDbContextFactory<PaymentDbContext>
    {
        public PaymentDbContext CreateDbContext(string[] args)
        {
            // 你可以把连接串硬编码，也可以读取环境变量
            const string cs = "server=localhost;port=3306;database=payment_db;user=root;password=1234";

            var opts = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseMySql(cs, new MySqlServerVersion(new Version(8,0,30)))
                .Options;

            return new PaymentDbContext(opts);
        }
    }
}

