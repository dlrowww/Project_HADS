using Microsoft.EntityFrameworkCore;
using OfferInventory.Application.Interfaces;
using OfferInventory.Application.Services;
using OfferInventory.Domain.Repositories;
using OfferInventory.Infrastructure.Data;
using OfferInventory.Infrastructure.Repositories;
using OfferInventory.Domain.Entities;   // ✅ 加这一行
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;


var builder = WebApplication.CreateBuilder(args);


// 控制器
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// DI 注册
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IOfferService, OfferService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddScoped<OfferInventory.Application.Services.FlixbusCrawler>();


builder.Services.AddHttpClient("flixbus", c =>
{
    c.BaseAddress = new Uri("https://global.api.flixbus.com/");
    c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
});

// ② 注册已有的单次爬虫
builder.Services.AddScoped<FlixbusCrawler>();

// ③ 把批量爬虫作为 HostedService
builder.Services.AddHostedService<FlixbusBatchCrawler>();

var app = builder.Build();   // ← 在这里 app 正式声明

// —— 把种子数据插入放在这里 —— 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
