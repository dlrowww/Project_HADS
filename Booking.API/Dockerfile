# -------- 运行时镜像 --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5088

# -------- 构建阶段 --------
FROM mcr.microsoft.com/dotnet/sdk:8.0.300 AS build
WORKDIR /src

# 复制解决方案，比单次 COPY . . 更利于缓存
COPY Booking.API/Booking.API.csproj Booking.API/
# 这里若有 Application、Domain、Infrastructure 也复制 csproj
COPY Booking.Application/Booking.Application.csproj Booking.Application/
COPY Booking.Domain/Booking.Domain.csproj Booking.Domain/
COPY Booking.Infrastructure/Booking.Infrastructure.csproj Booking.Infrastructure/

RUN dotnet restore Booking.API/Booking.API.csproj

# 再拷贝剩余源码
COPY . .
WORKDIR /src/Booking.API
RUN dotnet publish -c Release -o /app/publish

# -------- 最终镜像 --------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Booking.API.dll"]

