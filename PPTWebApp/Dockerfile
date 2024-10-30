# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore the dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the full project and publish it
COPY . .
RUN dotnet publish -c Release -o /app

# Stage 2: Set up the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Expose port 80 to the outside world
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "PPTWebApp.dll"]

