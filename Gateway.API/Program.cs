using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? throw new InvalidOperationException("Missing configuration: Jwt:Key");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/booking"))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

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

builder.Services.AddHttpClient("booking", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:BookingApi"]!);
});

// Program.cs  ——  Startup 逻辑
builder.Services.AddHttpClient("user", c =>
{
    // 从 appsettings.json 读，也支持 Docker 环境变量覆盖
    c.BaseAddress = new Uri(builder.Configuration["Services:UserApi"]!);
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();

// ───── 静态文件服务（允许访问 search.html）─────
app.UseStaticFiles();

// ───── 中间件顺序非常关键！─────
app.UseSwagger();
app.UseSwaggerUI();

// ✅ 启用 CORS 策略（必须在 MapControllers 之前）
app.UseCors("live-server");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapReverseProxy();  // ← 必须加这行！！


app.Run("http://0.0.0.0:5035");
