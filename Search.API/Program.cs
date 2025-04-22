using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// ✅ 精准 CORS 设置：只允许 localhost:5500 的网页访问
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5500")  // 确保和前端页面一致
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ 注册 HttpClient：用于请求 OfferInventory
builder.Services.AddHttpClient("offerInventory", c =>
{
    c.BaseAddress = new Uri("http://localhost:5189");  // OfferInventory.API 端口
    c.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Search.API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// ✅ 必须在 MapControllers 之前调用
app.UseCors();

app.UseAuthorization();
app.MapControllers();

app.Run();
