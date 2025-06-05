# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore packages
COPY . ./
RUN dotnet restore

# Publish the app to the out directory
RUN dotnet publish -c Release -o out

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

# Expose port (match the port in your launchSettings.json or appsettings)
EXPOSE 8080

ENTRYPOINT ["dotnet", "ExploreGambia.Api.dll"]
