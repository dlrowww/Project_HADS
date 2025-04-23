FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

WORKDIR /src/OfferInventory.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5189
ENV ASPNETCORE_URLS=http://+:5189

ENTRYPOINT ["dotnet", "OfferInventory.API.dll"]
