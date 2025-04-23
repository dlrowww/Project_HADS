using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OfferInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("live-server", p =>
        p.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// ───── HttpClient：指向 Search.API ─────
builder.Services.AddHttpClient("search", c =>
{
    c.BaseAddress = new Uri("http://localhost:5078"); // ← Search.API 实际端口
    c.Timeout = TimeSpan.FromSeconds(10);
});

// ───── 控制器 & Swagger ─────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway.API", Version = "v1" });
});


// ───── 如果网关也需要连库（可选）─────
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"]!;
  builder.Services.AddDbContext<AppDbContext>(options =>
      options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
var app = builder.Build();
// ───── 反向代理 ─────
app.UseStaticFiles(); 
// ───── 中间件顺序 ─────
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("live-server");   // ← 指定上面那条策略
app.MapControllers();

app.Run();
