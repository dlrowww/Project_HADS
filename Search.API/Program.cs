using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using OfferInventory.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 修改 CORS 设置：允许 Gateway 页面访问 Search.API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5035")  // Gateway 页面来源
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 注册 HttpClient：Search 调用 OfferInventory
builder.Services.AddHttpClient("offerInventory", c =>
{
    c.BaseAddress = new Uri("http://offerinventory:5189");  // Docker 容器名 + 端口
    c.Timeout = TimeSpan.FromSeconds(10);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 注册数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Search.API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// 注意调用顺序：CORS 必须在 MapControllers 之前
app.UseCors();

app.UseAuthorization();
app.MapControllers();

// 指定监听地址（推荐添加）
app.Run("http://0.0.0.0:5078");
