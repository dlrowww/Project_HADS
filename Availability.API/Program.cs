using Availability.Application.Services;
using Availability.Domain.Interfaces;
using Availability.Infrastructure.Data;
using Availability.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// 注册依赖服务
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();

// 使用内存数据库（开发测试阶段）
builder.Services.AddDbContext<AvailabilityDbContext>(options =>
    options.UseInMemoryDatabase("AvailabilityDb"));

// 添加 Swagger + MVC 控制器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers(); // 必须添加这句才能映射控制器路
