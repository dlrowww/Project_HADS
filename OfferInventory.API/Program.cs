using Microsoft.EntityFrameworkCore;
using OfferInventory.Application.Interfaces;            // IOfferService、IOfferScraper
using OfferInventory.Application.Services;              // OfferService
using OfferInventory.Domain.Repositories;               // IOfferRepository
using OfferInventory.Infrastructure.Data;               // AppDbContext
using OfferInventory.Infrastructure.Repositories;       // OfferRepository

var builder = WebApplication.CreateBuilder(args);

// 控制器
builder.Services.AddControllers();

// EF Core In-Memory
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TransportDb"));

// DI 注册
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IOfferService, OfferService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
