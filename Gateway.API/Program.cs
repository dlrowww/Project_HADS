// Gateway.API/Program.cs
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ 跨域：只允许你的 Live Server（5500）访问
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
          .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
          .AllowAnyMethod()
          .AllowAnyHeader();
    });
});

// 2️⃣ 注册 HttpClient："search" 指向 Search.API
builder.Services.AddHttpClient("search", c =>
{
    c.BaseAddress = new Uri("http://localhost:5078"); // ← Search.API 实际监听端口
    c.Timeout     = TimeSpan.FromSeconds(10);
});

// （可选）如果你还想直接拿 Inventory，也可以再加一个
builder.Services.AddHttpClient("offerInventory", c =>
{
    c.BaseAddress = new Uri("http://localhost:5189");
    c.Timeout     = TimeSpan.FromSeconds(10);
});

// 3️⃣ Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway.API", Version = "v1" });
});

var app = builder.Build();

// 中间件顺序：Swagger → CORS → 路由
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();           // 一定要在 MapControllers 之前
app.UseAuthorization();
app.MapControllers();

app.Run();
