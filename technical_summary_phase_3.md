# Ficha Técnica del Proyecto: RealEstateApp V2 — Fase 3 (Web API, Seguridad JWT y Swagger OpenAPI)

Este documento contiene un resumen técnico exhaustivo y de bajo nivel de todos los cambios, archivos creados, modificados y configurados durante la **Fase 3: Exposición de Servicios Internos y Seguridad JWT (Web API)** del proyecto **RealEstateApp V2**. Está estructurado para ser leído y procesado por cualquier Modelo de Lenguaje (IA) para su estudio o explicación al detalle.

---

## 🏗️ 1. Alcance de la Fase 3

La Fase 3 habilitó la capa de presentación Web API (`RealEstateApp.Presentation.WebApi`), asegurando el acceso mediante JWT (JSON Web Tokens), documentando la API con Swagger OpenAPI v1 y exponiendo controladores RESTful estructurados bajo la versión `v1`.

```
RealEstateApp (Solution)
 ├── src
 │    ├── Core
 │    │    ├── RealEstateApp.Core.Domain         ⭐ Agregado JWTSettings.cs
 │    │    └── RealEstateApp.Core.Application    ⭐ Agregado IAgentService, AgentService, DTO update
 │    ├── Infrastructure
 │    │    ├── RealEstateApp.Infrastructure.Persistence ⭐ Agregado AccountService con generación de JWT
 │    │    └── RealEstateApp.Infrastructure.Shared      ✅ Sin cambios (de Fase 2)
 │    └── Presentation
 │         ├── RealEstateApp.Presentation.WebApp        ✅ Sin cambios (Fase 4 próxima)
 │         └── RealEstateApp.Presentation.WebApi        ⭐ FOCO PRINCIPAL (Controllers v1, Program.cs, Swagger, JWT)
```

---

## 💾 2. Cambios en Detalle por Capas

### Capa 2.1: Core Domain (`RealEstateApp.Core.Domain`)

#### `Settings/JWTSettings.cs` (Nuevo)
Clase de configuración POCO utilizada para inyección con `IOptions<JWTSettings>`.
- **Propiedades**:
  - `Key` (string): Clave secreta simétrica para firmar el token HMAC-SHA256 (mínimo 256 bits).
  - `Issuer` (string): Emisor del token (`RealEstateAppIdentityApi`).
  - `Audience` (string): Destinatario/Audiencia del token (`RealEstateAppUser`).
  - `DurationInMinutes` (double): Tiempo de vida del token (default: 60 minutos).

---

### Capa 2.2: Core Application (`RealEstateApp.Core.Application`)

#### A. DTOs de Autenticación
- **`AuthenticationResponse.cs`** (Modificado):
  - Se eliminó el atributo `[JsonIgnore]` sobre `public string? JWToken { get; set; }`.
  - *Razón de ser*: Al autenticarse a través de la Web API (`POST /api/v1/account/authenticate`), el cliente REST debe recibir el JWT Token en el cuerpo JSON de la respuesta.

#### B. Interfaces de Servicio
- **`IAccountService.cs`** (Extendida):
  - Métodos incorporados:
    - `Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string role, string? origin = null)`: Registro flexible especificando el rol asignado.
    - `Task ChangeUserStatusAsync(string userId, bool isActive)`: Modificación del estado activo/bloqueado de una cuenta.
- **`IAgentService.cs`** (Nuevo):
  - Ubicación: `Interfaces/Services/IAgentService.cs`
  - Contrato: `GetAllViewModelAsync()`, `GetByIdViewModelAsync(id)`, `GetAllDtoAsync()`, `GetByIdDtoAsync(id)`, `ChangeStatusAsync(agentId, isActive)`.

#### C. Implementación de Servicios
- **`AgentService.cs`** (Nuevo):
  - Ubicación: `Services/AgentService.cs`
  - Inyecta `UserManager<IdentityUser>`, `IPropertyRepository`, `IAccountService`, `IMapper`.
  - `GetAllDtoAsync()` / `GetByIdDtoAsync()`: Consulta usuarios con rol `Agent` vía Identity, calcula el total de propiedades asignadas en `PropertyRepository`, determina el estado `IsActive` comprobando la propiedad `LockoutEnd` de `IdentityUser`, y mapea a `AgentDto` / `AgentPropertyDto`.
  - `ChangeStatusAsync(agentId, isActive)`: Verifica que el usuario exista y tenga el rol `Agent`, luego invoca `_accountService.ChangeUserStatusAsync(agentId, isActive)`.

#### D. Inyección de Dependencias — `ServiceRegistration.cs`
- Registrado `IAgentService → AgentService` como `Transient`.

---

### Capa 2.3: Infrastructure Persistence (`RealEstateApp.Infrastructure.Persistence`)

#### `Services/AccountService.cs` (Nuevo)
Implementación concreta del servicio de cuentas que interactúa con ASP.NET Core Identity y emite los tokens JWT.

- **Inyecciones**: `UserManager<IdentityUser>`, `RoleManager<IdentityRole>`, `IEmailService`, `IOptions<JWTSettings>`.
- **`AuthenticateAsync(AuthenticationRequest)`**:
  1. Busca al usuario por Email o UserName mediante `_userManager.FindByEmailAsync()` o `_userManager.FindByNameAsync()`.
  2. Valida la contraseña mediante `_userManager.CheckPasswordAsync()`.
  3. Verifica la confirmación del email (`user.EmailConfirmed`).
  4. Comprueba que el usuario no esté bloqueado o inactivo (`user.LockoutEnd > DateTimeOffset.UtcNow`).
  5. Obtiene los roles asociados (`_userManager.GetRolesAsync()`).
  6. Invoca el método privado `GenerateJwtTokenAsync(user)`.
  7. Retorna `AuthenticationResponse` con `JWToken` firmado y `HasError = false`.
- **`GenerateJwtTokenAsync(IdentityUser)`**:
  - Genera las Claims: `JwtRegisteredClaimNames.Sub`, `JwtRegisteredClaimNames.Jti` (GUID), `JwtRegisteredClaimNames.Email`, `ClaimTypes.NameIdentifier` (ID), `ClaimTypes.Name`, `ClaimTypes.Email`, y una Claim `ClaimTypes.Role` por cada rol del usuario.
  - Firma el token con `SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key))` y algoritmo `SecurityAlgorithms.HmacSha256`.
  - Establece expiración en `DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)`.
- **`ChangeUserStatusAsync(userId, isActive)`**:
  - Si `isActive == true`: `user.LockoutEnd = null`, `user.LockoutEnabled = false`.
  - Si `isActive == false`: `user.LockoutEnabled = true`, `user.LockoutEnd = DateTimeOffset.MaxValue`.
  - Actualiza mediante `_userManager.UpdateAsync(user)`.
- **`ServiceRegistration.cs`**:
  - Registrado `services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"))`.
  - Registrado `IAccountService → AccountService` como `Transient`.

---

### Capa 2.4: Presentation Web API (`RealEstateApp.Presentation.WebApi`)

#### A. Configuración del Proyecto y Paquetes (`csproj` y `appsettings.json`)
- **Paquetes NuGet adicionados**:
  - `Swashbuckle.AspNetCore` (v7.0.0): Habilita Swagger UI y la generación del documento `swagger.json`.
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (v10.0.10): Middleware para validación de encabezados `Authorization: Bearer <token>`.
- **`appsettings.json`**: Añadida la sección `"JWTSettings"`:
  ```json
  "JWTSettings": {
    "Key": "SuperSecretRealEstateAppJwtSigningKey2026SecureKeyWithAtLeast256Bits!",
    "Issuer": "RealEstateAppIdentityApi",
    "Audience": "RealEstateAppUser",
    "DurationInMinutes": 60
  }
  ```

#### B. Pipeline de Aplicación — `Program.cs`
- **Autenticación JWT Bearer**:
  - Registra `AddAuthentication()` con esquema por defecto `JwtBearerDefaults.AuthenticationScheme`.
  - Configura `TokenValidationParameters`:
    - `ValidateIssuerSigningKey = true`, `ValidateIssuer = true`, `ValidateAudience = true`, `ValidateLifetime = true`.
    - `ClockSkew = TimeSpan.Zero` (expiración exacta sin tolerancia adicional).
  - Manejo de eventos `JwtBearerEvents`:
    - `OnChallenge` (HTTP 401 Unauthorized): Retorna JSON estructurado `{ "hasError": true, "error": "No está autorizado para acceder a este recurso" }`.
    - `OnForbidden` (HTTP 403 Forbidden): Retorna JSON estructurado `{ "hasError": true, "error": "No tiene permisos suficientes para acceder a este recurso" }`.
- **Swagger / OpenAPI**:
  - Registra `AddSwaggerGen()` configurando `OpenApiInfo` (v1).
  - Agrega `AddSecurityDefinition("Bearer")` especificando esquema `ApiKey` en encabezado `Authorization`.
  - Agrega `AddSecurityRequirement()` para exigir el Bearer token en la interfaz gráfica de Swagger UI.
  - Habilita `app.UseSwagger()` y `app.UseSwaggerUI()` en la ruta `/swagger`.

#### C. Controladores RESTful (`Controllers/v1/`)

1. **`BaseApiController.cs`**:
   - Ruta base: `[Route("api/v1/[controller]")]`
   - Atributo `[ApiController]` para validación automática del `ModelState` y respuestas HTTP 400.

2. **`AccountController.cs`**:
   - `POST /api/v1/account/authenticate`: Endpoint público para inicio de sesión. Retorna `AuthenticationResponse` con el token JWT.
   - `POST /api/v1/account/register-admin`: `[Authorize(Roles = "Admin")]`. Registra un usuario con rol `Admin`.
   - `POST /api/v1/account/register-developer`: `[Authorize(Roles = "Admin")]`. Registra un usuario con rol `Developer`.
   - `GET /api/v1/account/confirm-email`: Endpoint público para confirmación de email mediante `userId` y `token`.

3. **`PropertiesController.cs`**:
   - `GET /api/v1/properties`: Endpoint público. Acepta query parameters (`PropertyFilterViewModel`) para filtrado combinado (precio, habitaciones, baños, tipo de propiedad, tipo de venta, código). Retorna `List<PropertyDto>`.
   - `GET /api/v1/properties/{id}`: Endpoint público. Retorna `PropertyDto` por ID o 404.
   - `GET /api/v1/properties/code/{code}`: Endpoint público. Retorna `PropertyDto` por código único de 6 dígitos o 404.

4. **`AgentsController.cs`**:
   - `GET /api/v1/agents`: Endpoint público. Retorna `List<AgentDto>`.
   - `GET /api/v1/agents/{id}`: Endpoint público. Retorna `AgentDto` por ID o 404.
   - `GET /api/v1/agents/{id}/properties`: Endpoint público. Retorna `List<AgentPropertyDto>` con el portafolio del agente.
   - `PATCH /api/v1/agents/{id}/change-status`: `[Authorize(Roles = "Admin")]`. Acepta `ChangeAgentStatusRequest { bool IsActive }` para activar/inactivar la cuenta del agente.

5. **`PropertyTypesController.cs`**:
   - `GET /api/v1/propertytypes`: Consulta pública de tipos de propiedad (`List<PropertyTypeDto>`).
   - `GET /api/v1/propertytypes/{id}`: Consulta pública por ID.
   - `POST /api/v1/propertytypes`: `[Authorize(Roles = "Admin,Developer")]`. Crear tipo de propiedad.
   - `PUT /api/v1/propertytypes/{id}`: `[Authorize(Roles = "Admin,Developer")]`. Modificar tipo de propiedad.
   - `DELETE /api/v1/propertytypes/{id}`: `[Authorize(Roles = "Admin,Developer")]`. Eliminar tipo de propiedad.

6. **`SaleTypesController.cs`**:
   - Mantenimiento CRUD protegido idéntico a `PropertyTypesController` (`SaleTypeDto`).

7. **`ImprovementsController.cs`**:
   - Mantenimiento CRUD protegido idéntico a `PropertyTypesController` (`ImprovementDto`).

---

## 🔒 3. Matriz de Seguridad y Roles por End-point

| Controlador | Endpoint | Método | Roles Permitidos |
|-------------|----------|--------|------------------|
| `Account` | `/api/v1/account/authenticate` | `POST` | **Público** |
| `Account` | `/api/v1/account/register-admin` | `POST` | `Admin` |
| `Account` | `/api/v1/account/register-developer` | `POST` | `Admin` |
| `Account` | `/api/v1/account/confirm-email` | `GET` | **Público** |
| `Properties` | `/api/v1/properties` | `GET` | **Público** |
| `Properties` | `/api/v1/properties/{id}` | `GET` | **Público** |
| `Properties` | `/api/v1/properties/code/{code}` | `GET` | **Público** |
| `Agents` | `/api/v1/agents` | `GET` | **Público** |
| `Agents` | `/api/v1/agents/{id}` | `GET` | **Público** |
| `Agents` | `/api/v1/agents/{id}/properties` | `GET` | **Público** |
| `Agents` | `/api/v1/agents/{id}/change-status` | `PATCH` | `Admin` |
| `PropertyTypes` | `/api/v1/propertytypes` | `GET` / `GET {id}` | **Público** |
| `PropertyTypes` | `/api/v1/propertytypes` | `POST` / `PUT` / `DELETE` | `Admin`, `Developer` |
| `SaleTypes` | `/api/v1/saletypes` | `GET` / `GET {id}` | **Público** |
| `SaleTypes` | `/api/v1/saletypes` | `POST` / `PUT` / `DELETE` | `Admin`, `Developer` |
| `Improvements` | `/api/v1/improvements` | `GET` / `GET {id}` | **Público** |
| `Improvements` | `/api/v1/improvements` | `POST` / `PUT` / `DELETE` | `Admin`, `Developer` |

---

## 🔍 4. Resultados de la Verificación

```
> dotnet build RealEstateApp.slnx
Build succeeded.
    14 Warning(s)   ← Pre-existentes (AutoMapper version mismatch, NuGet advisories)
    0 Error(s)
Time Elapsed 00:00:26.54
```

- Los 6 proyectos de la solución compilaron exitosamente.
- Swagger UI está accesible en `/swagger` habilitando pruebas interactivas con encabezado `Authorization: Bearer <token>`.
