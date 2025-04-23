FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

WORKDIR /src/Search.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5078
ENV ASPNETCORE_URLS=http://+:5078

ENTRYPOINT ["dotnet", "Search.API.dll"]
