using Booking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Booking.Application.CommandHandler;
using Booking.Application.Sagas;
using OfferInventory.Infrastructure.Data;  
using Booking.API.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5088");

// 读取连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 注册数据库上下文（MySQL）
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 42)),
        mysql => mysql.EnableRetryOnFailure()));

// 新增：池化的 DbContextFactory，专供后台线程 / Saga 使用
builder.Services.AddDbContextFactory<BookingDbContext>(options =>options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("OfferInventory"),
        new MySqlServerVersion(new Version(8, 0, 32))
    )
);

// 注册控制器
builder.Services.AddControllers();
var paymentApiUrl = builder.Configuration["Services:PaymentApi"];

builder.Services.AddSignalR();

var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? throw new InvalidOperationException("Missing configuration: Jwt:Key");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization();


if (string.IsNullOrWhiteSpace(paymentApiUrl))
{
    throw new InvalidOperationException("Missing configuration: Services:PaymentApi");
}

// ② 再安全地 new Uri
builder.Services.AddHttpClient("payment-api", client =>
{
    client.BaseAddress = new Uri(paymentApiUrl);
});
builder.Services.AddHttpClient("availability-api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AvailabilityApi"]
        ?? throw new InvalidOperationException("Missing configuration: Services:AvailabilityApi"));
});

// 注册 Swagger（用于接口测试）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CreateBookingHandler>();
builder.Services.AddScoped<BookingSagaCoordinator>();
// TODO: 注册你的 BookingService / Repository（稍后你实现了可以加上）

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.Migrate(); // 应用 Migrations
    Console.WriteLine("[EF] Database migration completed.");
}

// 开启 Swagger 中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用 HTTPS 与控制器路由
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BookingStatusHub>("/hubs/booking"); 
app.Run();
