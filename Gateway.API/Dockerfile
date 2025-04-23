FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 拷贝整个解决方案以包含引用项目
COPY . .

WORKDIR /src/Gateway.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5035
ENV ASPNETCORE_URLS=http://+:5035

ENTRYPOINT ["dotnet", "Gateway.API.dll"]
