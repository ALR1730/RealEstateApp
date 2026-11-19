# Contrato ALPHA — Fase 1: Integridad Arquitectónica (`BETA`) & Principios Turing

> **Fase:** 1 de 3 (`fase-1-arquitectura-turing`)  
> **Prerrequisito:** Fase 0 cerrada con gate `alr-zeta` en verde y `.NET 10 SDK` instalado localmente  
> **Migraciones de BD:** **1 migración** (`AddConcurrencyRowVersion` — ⚠️ requiere confirmación explícita de Angel Luis Rosario antes de aplicarse)  
> **Roles líderes:** Chief Architect (`BETA`) + Principal Engineer (`GAMMA`) + QA & TDD (`alr-qa-tdd`)

---

## 1. Sub-bloques y Archivos Afectados

### 1.1 Flujo Predecible (`Dijkstra`) & RFC 7807 (`Brooks`)
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Domain** | `Common/Result.cs`, `Common/Error.cs` *(Nuevos)* | Tipo discriminado `Result` / `Result<T>` y `Error` (`Code`, `Message`, `Type`: `Validation`, `NotFound`, `Conflict`, `Forbidden`, `Unauthorized`). |
| **Presentation.WebApi** | `Middleware/GlobalExceptionMiddleware.cs` *(Nuevo)* y `Filters/ApiGlobalExceptionFilter.cs` | Emisión uniforme de `ProblemDetails` (RFC 7807: `type`, `title`, `status`, `detail`, `instance`, `errors`) para errores `400/404/409/422/500`, capturando `DbUpdateConcurrencyException` (`409 Conflict`) y errores no controlados sin filtrar stack traces. |
| **Presentation.WebApi** | Controladores v1 (`AdminController`, `AgentsController`, `AiSearchController`, `AppointmentsController`, `OffersController`, `PropertiesController`, etc.) | Eliminar los 14 bloques `catch (Exception ex)` locales; devolver `ToActionResult()` desde `BaseApiController` mapeando `Result<T>` a HTTP + RFC 7807. |

### 1.2 Dominio Rico (`domain.md` — `Lampson`)
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Domain** | `Entities/Offer.cs`, `Property.cs`, `PropertyAppointment.cs`, `AgentSubscription.cs` | Encapsular setters (`private set`) en estados y claves de negocio; exponer métodos de transición de dominio que validen invariantes (`Offer.Accept()`, `Offer.Reject()`, `Property.MarkAsSold()`, `Property.AssignToAgent()`, etc.) e impidan mutación directa desde servicios/repositorios. |
| **Core.Domain** | `ValueObjects/Money.cs`, `ValueObjects/PropertyCode.cs` *(Nuevos)* | Value Objects inmutables con validación en constructor (`>= 0`, moneda válida, código alfanumérico de 6 caracteres). |
| **Core.Domain** | `Services/MortgageAmortizationCalculator.cs` *(Nuevo)* | Mover la fórmula matemática pura de amortización francesa fuera de `Infrastructure.Shared` hacia el dominio; `FinancingService` pasa a delegar en este dominio puro. |
| **Core.Application** | `Settings/JWTSettings.cs` (mover desde `Core.Domain/Settings/JWTSettings.cs`) | Retirar configuración de autenticación JWT de `Core.Domain`. |
| **Core.Domain / Application** | `Interfaces/Repositories/*` | Definir contratos de repositorio base en `Core.Domain` y mantener/adaptar compatibilidad limpia hacia `Core.Application`. |

### 1.3 Fronteras `BETA` y Atomicidad Transaccional
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Application** | `RealEstateApp.Core.Application.csproj` + `DTOs/Common/FileUploadDto.cs` *(Nuevo)* | **Eliminar `<FrameworkReference Include="Microsoft.AspNetCore.App" />`** de `Core.Application.csproj`. Reemplazar `IFormFile` en `IPropertyImageService`, `PropertyImageService`, `EditProfileViewModel`, `AgentVerificationViewModel`, `SaveOfferViewModel` y `SavePropertyViewModel` por `FileUploadDto` (`Stream Content, string FileName, string ContentType, long Length`) y mover `IdentityExtensions.cs` a `Infrastructure.Persistence`. |
| **Core.Application & WebApi** | `Controllers/v1/OwnersController.cs`, `VerificationsController.cs`, `SubscriptionsController.cs`, `BaseApiController.cs` | Eliminar `UserManager<IdentityUser>` de estos 3 controllers (usar `BaseApiController.CurrentUserId` desde `ClaimsPrincipal`). Mover la regla de negocio `MaxOwnerProperties = 2` fuera de `OwnersController` hacia `PropertyService` / invariante de dominio. |
| **Core.Application & Persistence** | `Services/OfferService.cs`, `Services/BuyAbilityService.cs`, `Repositories/OfferRepository.cs`, `Interfaces/IUnitOfWork.cs` | Hacer `IUnitOfWork` **obligatorio** (no nullable) en `OfferService` y `BuyAbilityService`. Sacar la lógica de cascada y las transacciones propias de `OfferRepository.AcceptOfferTransactionAsync` hacia `OfferService` orquestado por `IUnitOfWork.ExecuteTransactionAsync(...)`, incluyendo la transición previa de `AcceptCounterOffer` dentro de la misma transacción. |

### 1.4 Escalabilidad, Concurrencia y Paridad SQL Server/PostgreSQL (`Liskov`)
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Domain & Persistence** | `Entities/Property.cs`, `Entities/Offer.cs`, `ApplicationDbContext.cs`, nueva migración `AddConcurrencyRowVersion` | Añadir token de concurrencia optimista (`RowVersion`) configurado con `IsConcurrencyToken()`/`IsRowVersion()` compatible con SQL Server y PostgreSQL (`xmin` o columna versionada explícita). |
| **Core.Application & Persistence** | `DTOs/Common/PagedResult.cs` *(Nuevo)*, `Repositories/PropertyRepository.cs`, `Services/PropertyService.cs`, `Controllers/v1/PropertiesController.cs` | Paginación obligatoria (`Page >= 1`, `PageSize` máx `50`) en `GetAllAsync` y `GetWithFiltersAsync`; quitar `.Include(p => p.Offers)` y `.Include(p => p.Chats)` de `PropertyRepository.GetByIdAsync` para eliminar carga innecesaria y fuga de datos privados en catálogo público. |
| **Core.Application** | `Services/AiSearchService.cs` + `Services/AiSearch/*Extractors.cs` *(Nuevos)* | Extraer extractores por responsabilidad (`complejidad <= 10` por clase/método), cachear catálogos en `IMemoryCache` (TTL `5 min`) y fijar timeout `TimeSpan.FromMilliseconds(200)` en toda `Regex` contra ReDoS. |
| **Infrastructure.Persistence** | `Contexts/ApplicationDbContext.cs` + `Configurations/*Configuration.cs` *(Nuevos)* | Descomponer las `882` líneas de `OnModelCreating` en archivos `IEntityTypeConfiguration<T>` por entidad. |
| **Infrastructure.Persistence** | `ServiceRegistration.cs`, `Seeds/DefaultRealEstateData.cs` | Eliminar `.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))`; eliminar `Trust Server Certificate=true` hardcodeado en el parser de PostgreSQL (respetar configuración explícita); reemplazar `ExecuteSqlRawAsync("IF NOT EXISTS (SELECT * FROM sys.columns ...)")` de `DefaultRealEstateData.cs` por esquema gestionado exclusivamente vía migraciones EF Core. |

---

## 2. Criterios de Aceptación (`GIVEN / WHEN / THEN`)

1. **Integridad `BETA` sin Framework Web:**
   - `GIVEN` `RealEstateApp.Core.Application.csproj` y `RealEstateApp.Core.Domain.csproj`
   - `WHEN` se compila la solución con `dotnet build RealEstateApp.slnx`
   - `THEN` ninguno de los dos proyectos referencia `Microsoft.AspNetCore.App` ni paquetes `Microsoft.AspNetCore.*` / `Microsoft.EntityFrameworkCore.*`.
2. **RFC 7807 Uniforme (`ProblemDetails`):**
   - `GIVEN` cualquier error de validación, recurso inexistente, conflicto de concurrencia o fallo interno en cualquier controlador v1
   - `WHEN` responde la API
   - `THEN` el cuerpo cumple el esquema RFC 7807 (`type`, `title`, `status`, `detail`, `instance`) con código HTTP exacto (`400`/`422`, `404`, `409`, `500`) y sin filtrar `Exception.Message` internos en `500`.
3. **Concurrencia Optimista en Ofertas:**
   - `GIVEN` dos peticiones concurrentes intentando aceptar distintas ofertas sobre la misma propiedad `Available`
   - `WHEN` la primera completa la transacción y marca la propiedad como `Sold`
   - `THEN` la segunda falla con conflicto de concurrencia (`409 Conflict`) sin emitir doble comisión ni dejar dos ofertas aceptadas.
4. **Encapsulación de Dominio:**
   - `GIVEN` una entidad `Offer` en estado `Rejected` o `Accepted`
   - `WHEN` un servicio intenta aceptarla o contraofertarla
   - `THEN` las propiedades de estado no permiten asignación directa externa (`private set`) y el método de dominio rechaza la transición inválida.
5. **Paginación y Límite de Carga:**
   - `GIVEN` `GET /api/v1/properties?page=1&pageSize=200` (o sin parámetros)
   - `WHEN` consulta el repositorio
   - `THEN` limita `PageSize <= 50` en SQL (`Skip`/`Take`), retorna `PagedResult<T>` y la consulta pública por ID no carga `Offers` ni `Chats`.
6. **Paridad PostgreSQL / SQL Server:**
   - `GIVEN` el arranque contra PostgreSQL o SQL Server
   - `WHEN` se ejecutan migraciones y `DefaultRealEstateData.SeedAsync`
   - `THEN` no se ejecuta ninguna sentencia `SELECT * FROM sys.columns` ni SQL específico de proveedor.

---

## 3. Riesgos y Confirmaciones Requeridas

- **⚠️ Confirmación explícita requerida (Guardarraíl #1):** antes de crear o aplicar `AddConcurrencyRowVersion`, **Angel Luis Rosario** debe aprobar el cambio de esquema en `Properties` y `Offers` y su estrategia para SQL Server (`rowversion`) / PostgreSQL (`xmin` o `Version` entero).
- Cambiar `GetAllAsync`/`GetWithFiltersAsync` a respuestas paginadas requiere coordinar la firma de respuesta con `PropertiesCatalogPage.tsx` en `ClientApp` (o proveer wrapper retrocompatible temporal en el cliente API durante la transición).
