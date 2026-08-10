FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5086
EXPOSE 5086

FROM mcr.microsoft.com/dotnet/sdk:8.0.300 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Availability.API/Availability.API.csproj"
RUN dotnet build "Availability.API/Availability.API.csproj" -c Release -o /app/build
RUN dotnet publish "Availability.API/Availability.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Availability.API.dll"]
