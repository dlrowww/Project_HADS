using Microsoft.EntityFrameworkCore;
using OfferInventory.Application.Services;
using OfferInventory.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 注册 EF Core In-Memory 数据库
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TransportDb"));
    
builder.Services.AddHttpClient("OfferInventory", client =>
{
    client.BaseAddress = new Uri("http://localhost:5189"); // OfferInventory API 地址
});


// 注册服务
builder.Services.AddScoped<IOfferService, OfferInventory.Infrastructure.Services.OfferService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();
// 这段代码是一个 ASP.NET Core Web API 的启动文件，