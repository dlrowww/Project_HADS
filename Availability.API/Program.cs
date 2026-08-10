using Microsoft.EntityFrameworkCore;
using Availability.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5086");

// 1. 注册控制器
builder.Services.AddControllers();

// 2. 注册 Swagger（可选，但推荐）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AvailabilityDbContext>(opt =>
    opt.UseMySql(cs,
        new MySqlServerVersion(new Version(8, 0, 42)),   // 
        mysql => mysql.EnableRetryOnFailure()));

builder.Services.AddHostedService<SeatLockExpirationService>();

var app = builder.Build();

// 4. 在应用启动时自动执行 EF Core 迁移（如果尚未创建数据库或表，就自动创建）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AvailabilityDbContext>();
    db.Database.Migrate();
}

// 5. 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // 注意：你需要在 Controllers 里至少有个 Test Controller

app.Run();
