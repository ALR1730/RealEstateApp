# Multi-stage Dockerfile para RealEstateApp WebApi (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copiar archivos de proyecto (.csproj) para aprovechar el caché de capas de Docker
COPY src/Core/RealEstateApp.Core.Domain/RealEstateApp.Core.Domain.csproj src/Core/RealEstateApp.Core.Domain/
COPY src/Core/RealEstateApp.Core.Application/RealEstateApp.Core.Application.csproj src/Core/RealEstateApp.Core.Application/
COPY src/Infrastructure/RealEstateApp.Infrastructure.Shared/RealEstateApp.Infrastructure.Shared.csproj src/Infrastructure/RealEstateApp.Infrastructure.Shared/
COPY src/Infrastructure/RealEstateApp.Infrastructure.Persistence/RealEstateApp.Infrastructure.Persistence.csproj src/Infrastructure/RealEstateApp.Infrastructure.Persistence/
COPY src/Presentation/RealEstateApp.Presentation.WebApi/RealEstateApp.Presentation.WebApi.csproj src/Presentation/RealEstateApp.Presentation.WebApi/

# 2. Restaurar dependencias NuGet
RUN dotnet restore src/Presentation/RealEstateApp.Presentation.WebApi/RealEstateApp.Presentation.WebApi.csproj

# 3. Copiar todo el código fuente y compilar para Release
COPY src/ src/
RUN dotnet publish src/Presentation/RealEstateApp.Presentation.WebApi/RealEstateApp.Presentation.WebApi.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# 4. Imagen final ultraligera de producción
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render expone la variable de entorno PORT dinámicamente (por defecto 8080 o 10000)
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RealEstateApp.Presentation.WebApi.dll"]
