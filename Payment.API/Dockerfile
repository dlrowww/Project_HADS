# 运行时基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# 关键：让 Kestrel 绑定到 5089，而不是默认 8080
ENV ASPNETCORE_URLS=http://+:5089
EXPOSE 5089

# ----------------------------------------------------

# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制 csproj 并还原依赖
COPY Payment.API/Payment.API.csproj              Payment.API/
COPY Payment.Application/Payment.Application.csproj Payment.Application/
COPY Payment.Domain/Payment.Domain.csproj           Payment.Domain/
COPY Payment.Infrastructure/Payment.Infrastructure.csproj Payment.Infrastructure/

RUN dotnet restore Payment.API/Payment.API.csproj

# 拷贝源码 & 编译
COPY . .
WORKDIR /src/Payment.API
RUN dotnet build Payment.API.csproj -c Release -o /app/build

# 发布阶段
FROM build AS publish
RUN dotnet publish Payment.API.csproj -c Release -o /app/publish

# ----------------------------------------------------

# 最终运行镜像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Payment.API.dll"]
