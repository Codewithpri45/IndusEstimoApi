# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY src/*.sln ./
COPY src/IndasEstimo.Api/*.csproj ./IndasEstimo.Api/
COPY src/IndasEstimo.Application/*.csproj ./IndasEstimo.Application/
COPY src/IndasEstimo.Domain/*.csproj ./IndasEstimo.Domain/
COPY src/IndasEstimo.Infrastructure/*.csproj ./IndasEstimo.Infrastructure/
COPY src/IndasEstimo.Shared/*.csproj ./IndasEstimo.Shared/
COPY src/IndasEstimo.SetupUtility/*.csproj ./IndasEstimo.SetupUtility/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY src/ ./

# Build the application
WORKDIR /src/IndasEstimo.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "IndasEstimo.Api.dll"]
