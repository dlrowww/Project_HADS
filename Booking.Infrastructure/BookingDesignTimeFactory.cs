using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Booking.Domain.Entities;

namespace Booking.Infrastructure
{
    public class BookingDesignTimeFactory : IDesignTimeDbContextFactory<BookingDbContext>
    {
        public BookingDbContext CreateDbContext(string[] args)
        {
            // 你可以把连接串硬编码，也可以读取环境变量
            const string cs = "server=localhost;port=3306;database=booking_db;user=root;password=1234";

            var opts = new DbContextOptionsBuilder<BookingDbContext>()
                .UseMySql(cs, new MySqlServerVersion(new Version(8,0,30)))
                .Options;

            return new BookingDbContext(opts);
        }
    }
}

