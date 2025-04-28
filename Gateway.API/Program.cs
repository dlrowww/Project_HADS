using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OfferInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ───── CORS 设置（如果前端运行在外部端口，比如 live-server）─────
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("live-server", p =>
        p.WithOrigins(
            "http://127.0.0.1:5500",
            "http://localhost:5500",
            "http://localhost:5035"      // 添加自己容器网页的来源
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ───── HttpClient：用于反向代理到 Search.API ─────
builder.Services.AddHttpClient("search", c =>
{
    c.BaseAddress = new Uri("http://search:5078");  // Docker 容器内的服务名
    c.Timeout = TimeSpan.FromSeconds(10);
});

// ───── 控制器 & Swagger ─────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway.API", Version = "v1" });
});

// ───── 如果 Gateway 也要连接数据库（可选）─────
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}

var app = builder.Build();

// ───── 静态文件服务（允许访问 search.html）─────
app.UseStaticFiles();

// ───── 中间件顺序非常关键！─────
app.UseSwagger();
app.UseSwaggerUI();

// ✅ 启用 CORS 策略（必须在 MapControllers 之前）
app.UseCors("live-server");

app.UseAuthorization();
app.MapControllers();

app.Run("http://0.0.0.0:5035");
