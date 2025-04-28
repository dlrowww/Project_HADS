using Microsoft.EntityFrameworkCore;
using OfferInventory.Application.Interfaces;
using OfferInventory.Application.Services;
using OfferInventory.Domain.Repositories;
using OfferInventory.Infrastructure.Data;
using OfferInventory.Infrastructure.Repositories;
using OfferInventory.Domain.Entities;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 添加控制器
builder.Services.AddControllers();

// 数据库连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 配置 EF Core 使用 MySQL（Pomelo 提供）
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 依赖注入
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IOfferService, OfferService>();

builder.Services.AddScoped<FlixbusCrawler>();
builder.Services.AddHostedService<FlixbusBatchCrawler>();

// HttpClient（默认和 FlixBus API）
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("flixbus", c =>
{
    c.BaseAddress = new Uri("https://global.api.flixbus.com/");
    c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
});

// Swagger 接口文档
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//  启动时执行迁移 + 可选插入数据
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 自动迁移：根据 Migrations 建立/更新表结构（如 TransportOffers）
    db.Database.Migrate();

    // 可选：种子数据插入
    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Username = "admin",
            Password = "1234"
        });
        db.SaveChanges();
    }
}

// 启用 Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// 映射控制器
app.MapControllers();

// 启动应用
app.Run();
