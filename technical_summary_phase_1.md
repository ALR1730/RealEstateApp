# Ficha Técnica del Proyecto: RealEstateApp V2 — Fase 1 (Identidad, Mapeos y Semillas)

Este documento contiene un resumen técnico exhaustivo y de bajo nivel de todos los cambios, archivos creados, modificados y configurados durante la **Fase 1: Configuración de Identidad y Semillas de Datos (Persistencia)** del proyecto **RealEstateApp V2**. Está estructurado para ser leído y procesado por cualquier Modelo de Lenguaje (IA) para su estudio o explicación al detalle.

---

## 🏗️ 1. Arquitectura del Sistema
El proyecto implementa una **Onion Architecture** (Arquitectura de Cebolla) con 6 capas desacopladas, lo que aísla las reglas de negocio del framework y los sistemas de almacenamiento.

```
RealEstateApp (Solution)
 ├── src
 │    ├── Core
 │    │    ├── RealEstateApp.Core.Domain         (Entidades de dominio puras, Enums, sin dependencias externas)
 │    │    └── RealEstateApp.Core.Application    (Interfaces de servicio/repo, DTOs, ViewModels, lógica de aplicación)
 │    ├── Infrastructure
 │    │    ├── RealEstateApp.Infrastructure.Persistence (DbContext, Migraciones, Repositorios, Seeds, Identity)
 │    │    └── RealEstateApp.Infrastructure.Shared      (Servicios cruzados: Email, Cloud Storage, Pagos)
 │    └── Presentation
 │         ├── RealEstateApp.Presentation.WebApp        (UI basada en ASP.NET Core MVC 10)
 │         └── RealEstateApp.Presentation.WebApi        (Servicios REST protegidos por JWT)
```

---

## 💾 2. Cambios en Detalle por Capas

### Capa 2.1: RealEstateApp.Core.Domain (Entidades y Enums)

#### A. Nuevas Entidades
1. **`PropertyImprovement.cs`** (Entidad de asociación muchos-a-muchos explícita entre `Property` e `Improvement`).
   - *Razón de ser*: EF Core permite relaciones implícitas, pero definirla explícitamente permite tener control total sobre los mapeos y añadir campos de auditoría o propiedades en la tabla puente en el futuro.
   - *Campos*: `PropertyId` (int), `Property` (objeto navegación), `ImprovementId` (int), `Improvement` (objeto navegación).

2. **`Favorite.cs`** (Hereda de `AuditableBaseEntity`).
   - *Razón de ser*: Representa la relación entre el Cliente y sus propiedades favoritas.
   - *Campos*: `ClienteId` (string, mapeado al Id de usuario de Identity) y `PropertyId` (int) con su objeto de navegación `Property`.

#### B. Modificaciones en Entidades Existentes
1. **`Property.cs`**
   - Se cambió la relación directa de muchos a muchos `ICollection<Improvement> Improvements` por la colección intermedia `ICollection<PropertyImprovement> PropertyImprovements`.
   - Se agregaron las colecciones de navegación inversa `ICollection<Favorite> Favorites` e `ICollection<MortgageSimulation> MortgageSimulations`.
   - Campos de geolocalización espacial y financieros agregados (Fase 1): `Latitude` (double), `Longitude` (double), `VideoUrl` (string?), `Tour360Url` (string?), `MontoSeparacion` (decimal), `PorcentajeInicialRequerido` (int).

2. **`Improvement.cs`**
   - Se cambió `ICollection<Property> Properties` por `ICollection<PropertyImprovement> PropertyImprovements`.

---

### Capa 2.2: RealEstateApp.Core.Application (Contratos, Interfaces y DTOs)

Para preparar los endpoints de API y el login en la WebApp de forma limpia, se definió la abstracción de cuenta.

#### A. DTOs de Autenticación (`DTOs/Account/`)
1. **`AuthenticationRequest.cs`**:
   - Campos: `Email` (string), `Password` (string).
2. **`AuthenticationResponse.cs`**:
   - Campos: `Id`, `UserName`, `Email`, `Roles` (List<string>), `IsVerified` (bool), `HasError` (bool), `Error` (string?), `JWToken` (string? marcado con `[JsonIgnore]` para que no viaje en el JSON serializado por defecto).
3. **`RegisterRequest.cs`**:
   - Validaciones DataAnnotations: `FirstName` (Requerido), `LastName` (Requerido), `Email` (Requerido y tipo Email), `UserName` (Requerido), `Password` (Requerido, tipo Password) y `ConfirmPassword` (Requerido, comparado con Password).
4. **`RegisterResponse.cs`**:
   - Campos: `Id`, `UserName`, `Email`, `HasError`, `Error`.

#### B. Interface de Servicio de Cuentas
1. **`IAccountService.cs`**:
   - Métodos asíncronos abstractos:
     - `Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)`
     - `Task<RegisterResponse> RegisterBasicUserAsync(RegisterRequest request)`
     - `Task<string> ConfirmAccountAsync(string userId, string token)`
     - `Task SignOutAsync()`

---

### Capa 2.3: RealEstateApp.Infrastructure.Persistence (Acceso a Datos e Identity)

Esta es la capa donde recae el 80% de los cambios de la Fase 1.

#### A. Mapeo en el DbContext (`ApplicationDbContext.cs`)
1. **Herencia**: Hereda de `IdentityDbContext` en lugar de `DbContext` básico.
2. **Tablas de Identity Personalizadas**:
   - Para no usar las tablas predeterminadas de ASP.NET Core (`AspNetUsers`, `AspNetRoles`, etc.), se renombraron en `OnModelCreating`:
     - `IdentityUser` -> `Users`
     - `IdentityRole` -> `Roles`
     - `IdentityUserRole<string>` -> `UserRoles`
     - `IdentityUserLogin<string>` -> `UserLogins`
3. **Configuración Fluent API por Entidad**:
   - **`Property`**: Clave primaria. `Code` es requerido, longitud máxima 6, e índice único. `Price` y `MontoSeparacion` configurados con tipo de columna SQL `decimal(18,2)`. Relación uno-a-muchos con `PropertyType` y `SaleType` configurada explícitamente con `DeleteBehavior.Restrict` para evitar la eliminación accidental en cascada de inmuebles enteros si se borra un tipo de propiedad.
   - **`PropertyImage`**: Longitud máxima de URL 500, relación en cascada con `Property`.
   - **`PropertyImprovement`**: Clave primaria compuesta por `(PropertyId, ImprovementId)`. Relaciones en cascada para limpiar la tabla intermedia automáticamente al borrar propiedades o mejoras.
   - **`Favorite`**: Índice único compuesto `(ClienteId, PropertyId)` para evitar que un mismo cliente guarde dos veces el mismo favorito.
   - **`MortgageSimulation`**: Mapeos decimales de precisión y claves foráneas en cascada.
4. **Auditoría Automática**:
   - Se sobrescribió `SaveChangesAsync()`. Al guardar cambios, EF Core intercepta las entidades que heredan de `AuditableBaseEntity` y:
     - Si la entidad está en estado `Added`: Configura `Created = DateTime.UtcNow` y `CreatedBy = "System"`.
     - Si está en estado `Modified`: Configura `LastModified = DateTime.UtcNow` y `LastModifiedBy = "System"`.

#### B. Semillas de Datos (Seeds)
Se crearon clases estáticas bajo la carpeta `Seeds/` que se inyectan en el startup de la aplicación.
1. **`DefaultRoles.cs`**:
   - Utiliza `RoleManager<IdentityRole>`.
   - Crea si no existen los roles: `Admin`, `Agent`, `Client`, `Developer`.
2. **`DefaultAdminUser.cs`**:
   - Crea a `admin@realestate.com` (password: `Admin123!`), marca `EmailConfirmed = true` y le asigna el rol `Admin`.
3. **`DefaultAgentUser.cs`**, **`DefaultClientUser.cs`**, **`DefaultDeveloperUser.cs`**:
   - Crean los respectivos usuarios semilla (`agent@realestate.com` / `Agent123!`, etc.) con email confirmado y sus roles respectivos asignados.

#### C. Inyección de Dependencias (`ServiceRegistration.cs`)
- **Base de Datos**: Lee `UseInMemoryDatabase` del config. Si es false, inicializa SQL Server usando la cadena de conexión predeterminada y habilita **NetTopologySuite** (`m => m.UseNetTopologySuite()`) para soporte espacial de EF.
- **Identity**: Configuración detallada:
  - Contraseñas: Al menos 1 dígito, 1 minúscula, 1 mayúscula, 1 caracter especial, mínimo 6 caracteres.
  - Bloqueo: Máximo 5 intentos fallidos, bloquea la cuenta por 5 minutos.
  - Email único: `options.User.RequireUniqueEmail = true`.
  - Confirmación obligatoria: `options.SignIn.RequireConfirmedEmail = true` (exigido por las especificaciones).
  - Almacén de base de datos y proveedores de tokens por defecto agregados.

#### D. Archivo del Proyecto (`RealEstateApp.Infrastructure.Persistence.csproj`)
- Se agregaron las siguientes dependencias NuGet:
  - `Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite` (v10.0.10) para el soporte espacial.
  - `Microsoft.EntityFrameworkCore.Tools` (v10.0.10) para habilitar herramientas de migración por consola.
- Se agregó la referencia de framework:
  - `<FrameworkReference Include="Microsoft.AspNetCore.App" />` para posibilitar el uso de extensiones web como `AddIdentity` y `AddEntityFrameworkStores` dentro de una biblioteca de clases puro.

---

### Capa 2.4: Presentation (Proyectos de Inicio WebApp y WebApi)

#### A. WebApp MVC (`Program.cs` y configuración)
- **Modificaciones en `Program.cs`**:
  - Inyección de dependencias en orden: `AddApplicationLayer()`, `AddPersistenceInfrastructure()`, `AddSharedInfrastructure()`.
  - Canalización HTTP: Inserción de `app.UseAuthentication()` justo antes de `app.UseAuthorization()` para asegurar el control de accesos.
  - **Ejecución de Semillas**: Al arrancar la aplicación, crea un `IServiceScope`, resuelve `RoleManager` y `UserManager`, y ejecuta en cascada los seeds de roles y usuarios por defecto dentro de un bloque `try-catch`.
- **Configuración (`appsettings.json`)**:
  - Añadida la cadena de conexión local: `Server=(localdb)\\MSSQLLocalDB;Database=RealEstateAppDb;Trusted_Connection=True;MultipleActiveResultSets=true`.
  - Agregado el paquete `Microsoft.EntityFrameworkCore.Design` (v10.0.10) en la WebApp (el proyecto ejecutable) para posibilitar que la herramienta `dotnet-ef` genere las migraciones desde este proyecto.

#### B. WebApi (`Program.cs` y configuración)
- **Modificaciones en `Program.cs`**:
  - Eliminado el endpoint demo `WeatherForecast`.
  - Añadido el registro de capas del proyecto, pipeline de autenticación/autorización y los seeds de usuarios y roles al startup.
- **Configuración (`appsettings.json` y csproj)**:
  - Añadida la cadena de conexión LocalDB idéntica a la de la WebApp.
  - Agregada referencia al proyecto `RealEstateApp.Core.Application` para poder usar DTOs.

---

## 🗄️ 3. Estado de la Base de Datos y Verificación de la Migración

La migración inicial `20260715215121_InitialCreate.cs` fue generada con éxito. Al aplicar la migración al LocalDB mediante `dotnet ef database update`, las siguientes tablas fueron creadas físicamente en SQL Server LocalDB:

1. **Tablas del Sistema (Identity)**:
   - `Users` (Información de cuentas, emails y contraseñas hasheadas).
   - `Roles` (Roles del sistema).
   - `UserRoles` (Tabla de relación muchos-a-muchos entre usuarios y roles).
   - `UserLogins`, `UserClaims`, `RoleClaims`, `UserTokens` (Tablas auxiliares de Identity).
2. **Tablas de Negocio (Dominio)**:
   - `Properties` (Campos de propiedad, geolocalización, código de 6 dígitos único con índice).
   - `PropertyImages` (URLs de imágenes vinculadas a propiedades).
   - `PropertyTypes` y `SaleTypes` (Catálogos básicos).
   - `Improvements` (Mejoras).
   - `PropertyImprovements` (Tabla join para relación muchos a muchos con claves compuestas).
   - `Favorites` (Favoritos de clientes con índice compuesto único para evitar duplicados).
   - `Offers` (Ofertas de separación y estados).
   - `Chats` (Hilos de mensajería).
   - `MortgageSimulations` (Cálculo amortización).

### 🔍 Resultados de las Pruebas de Ejecución:
- La base de datos se crea correctamente en LocalDB.
- Al iniciar `RealEstateApp.Presentation.WebApp` en http://localhost:5080, la consola de Entity Framework muestra la inserción correcta de las semillas de roles y usuarios en SQL.
- El servidor responde correctamente con la interfaz web inicial (código de respuesta 200 HTTP).
