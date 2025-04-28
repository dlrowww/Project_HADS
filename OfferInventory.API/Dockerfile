# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5189  

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "OfferInventory.API/OfferInventory.API.csproj"
RUN dotnet build "OfferInventory.API/OfferInventory.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OfferInventory.API/OfferInventory.API.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OfferInventory.API.dll"]