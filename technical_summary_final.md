# Ficha Técnica Final del Proyecto: RealEstateApp V2 — Documentación Integral (Fases 1 a 6)

Este documento contiene un resumen técnico exhaustivo y de bajo nivel de toda la solución del proyecto **RealEstateApp V2**, estructurado bajo la arquitectura **Onion (Clean Architecture)** con soporte para ASP.NET Core MVC (Web Portal), ASP.NET Core Web API (RESTful JWT), Entity Framework Core (SQL Server con NetTopologySuite Spatial), ASP.NET Core Identity y AutoMapper.

---

## 🏗️ 1. Arquitectura del Sistema (Onion Architecture)

```
RealEstateApp (Solution)
 ├── src
 │    ├── Core
 │    │    ├── RealEstateApp.Core.Domain         (Entidades de Dominio, Enums, Interfaces Repositorio Core)
 │    │    └── RealEstateApp.Core.Application    (DTOs, ViewModels, Services, AutoMapper Profiles, ServiceRegistration)
 │    ├── Infrastructure
 │    │    ├── RealEstateApp.Infrastructure.Persistence (ApplicationDbContext, EF Core Configurations, Identity, Seeds, Repositories)
 │    │    └── RealEstateApp.Infrastructure.Shared      (FileStorageService, FinancingService Amortización Francesa, PaymentService)
 │    └── Presentation
 │         ├── RealEstateApp.Presentation.WebApi        (RESTful API v1 Controllers, JWT Bearer Auth, Swashbuckle OpenAPI)
 │         └── RealEstateApp.Presentation.WebApp        (MVC WebApp, Auth Cookies, Custom Design System shadcn/ui, Controllers & Razor Views)
```

---

## 🔐 2. Controles de Seguridad Cruzada (Fase 6)

1. **Restricción en WebApp MVC**:
   - En `WebApp/Controllers/AccountController.cs`, durante el inicio de sesión se valida si la cuenta pertenece al rol `Developer`. Si es así, se niega el acceso indicando:
     > *"Acceso Denegado: Las cuentas con rol Desarrollador están restringidas exclusivamente al consumo de la Web API REST."*

2. **Restricción en Web API REST**:
   - En `WebApi/Controllers/v1/AccountController.cs`, durante el `POST /api/v1/Account/authenticate` se valida si el usuario posee roles `Client` o `Agent`. Si es así, la API responde un `400 Bad Request` indicando:
     > *"Acceso Denegado: Las cuentas con rol Cliente o Agente no tienen permitido el acceso a los servicios REST de la Web API."*

---

## 📋 3. Resumen por Fases del Plan de Trabajo

| Fase | Título | Alcance Implementado |
|------|--------|----------------------|
| **Fase 1** | Arquitectura y Entidades de Dominio | Solución en capas Onion, entidades de Dominio (`Property`, `PropertyType`, `SaleType`, `Improvement`, `PropertyImprovement`, `PropertyImage`, `Favorite`, `Offer`, `Chat`), DbContext, Spatial (`NetTopologySuite`) y Seeds de Usuarios y Roles. |
| **Fase 2** | Capa de Infraestructura y Aplicación | `FinancingService` (Amortización Francesa), `FileStorageService`, DTOs con DataAnnotations, Mapeos AutoMapper y Repositorios Genéricos y Específicos con Eager Loading. |
| **Fase 3** | Seguridad JWT y Web API REST | Configuración JWT Bearer (`JWTSettings`), Swashbuckle OpenAPI / Swagger UI en raíz, y Controllers REST v1 (`Account`, `Properties`, `Agents`, `PropertyTypes`, `SaleTypes`, `Improvements`) con parches `PATCH` restringidos a Admins. |
| **Fase 4** | Portal Web MVC y Experiencia Cliente | Sistema visual shadcn/ui + Bootstrap 5 + Vanilla CSS (`site.css`), Identidad Cookie MVC, Catálogo con búsqueda por código de 6 dígitos, Filtros combinados, Detalle con carrusel, tour 360, video, Simulador Hipotecario en JS, Directorio de Agentes, Mis Favoritos, Mis Ofertas (con adjuntos bancarios) e Hilos de Chat. |
| **Fase 5** | Panel de Agente y Administrador | CRUD de propiedades (hasta 15 fotos, geolocalización), **Regla Atómica de Ofertas** (Aceptar → Propiedad *Vendida* → Rechazo en cascada), **Dashboard Admin de KPIs en tiempo real**, mantenimientos backoffice y **Eliminación Física en Cascada de Agentes** (disco + BD). |
| **Fase 6** | Pruebas, Optimización y Entrega | Controles de Seguridad Cruzada, compilación limpia sin errores ni advertencias y documentación técnica final. |

---

## ⚡ 4. Comandos de Ejecución

### Ejecutar la Aplicación Web MVC
```bash
dotnet run --project src/Presentation/RealEstateApp.Presentation.WebApp
```
- **URL**: `http://localhost:5000` o la asignada por `launchSettings.json`.

### Ejecutar la Web API REST
```bash
dotnet run --project src/Presentation/RealEstateApp.Presentation.WebApi
```
- **URL Swagger UI**: `http://localhost:5196/`

---

## 🔍 5. Estado Final del Proyecto

- **Compilación de la Solución**: `Build succeeded` (**0 Error(s)**, **0 Warning(s)**).
- **Cobertura de Requerimientos**: **100% de la hoja de ruta (`roadmap.md`) completada**.
