# Ficha Técnica del Proyecto: RealEstateApp V2 — Estado Inicial (Antes de la Fase 1)

Este documento contiene un resumen técnico exhaustivo del estado del proyecto **RealEstateApp V2** antes de que se iniciaran los cambios de la **Fase 1** (es decir, el estado en el que se encontraba el repositorio originalmente). Está diseñado para ser leído y procesado por cualquier Modelo de Lenguaje (IA) para entender el punto de partida del desarrollo.

---

## 🏗️ 1. Arquitectura Base y Proyectos
La solución ya estaba estructurada usando **Onion Architecture** y contenía los siguientes proyectos de base:

* **`RealEstateApp.Core.Domain`**: Biblioteca de clases de .NET 10. Contiene las entidades puras del negocio, sin dependencias a bases de datos ni frameworks.
* **`RealEstateApp.Core.Application`**: Biblioteca de clases de .NET 10. Contiene los contratos (interfaces) de repositorios y servicios, perfiles de AutoMapper y ViewModels.
* **`RealEstateApp.Infrastructure.Persistence`**: Biblioteca de clases de .NET 10. Contiene el DbContext de Entity Framework Core, repositorios genéricos y registro de servicios.
* **`RealEstateApp.Infrastructure.Shared`**: Biblioteca de clases de .NET 10. Mocks o implementaciones simuladas de servicios externos (Email y almacenamiento de archivos).
* **`RealEstateApp.Presentation.WebApp`**: Aplicación web MVC (.NET 10) que sirve como portal de usuario.
* **`RealEstateApp.Presentation.WebApi`**: API REST (.NET 10) que sirve de puente para integraciones externas y desarrolladores.

---

## 💾 2. Estructura de Capas Inicial (Pre-Fase 1)

### Capa 2.1: RealEstateApp.Core.Domain (Entidades Iniciales)

#### A. Entidades del Dominio
Las entidades principales heredaban de `AuditableBaseEntity` y estaban definidas como sigue:

1. **`Property.cs`**:
   - Campos básicos: `Code` (código de inmueble), `Price` (precio), `Rooms` (habitaciones), `Bathrooms` (baños), `SizeInMeters` (tamaño), `Description` (descripción), `AgentId` (identificador del agente) y `Status` (Disponible, Reservada, Vendida).
   - Relaciones iniciales:
     - `PropertyTypeId` e `PropertyType` (Tipo de propiedad, e.g., Casa, Apartamento).
     - `SaleTypeId` y `SaleType` (Tipo de operación, e.g., Venta, Alquiler).
     - Relaciones implícitas muchos-a-muchos: `ICollection<Improvement> Improvements`.
     - Colecciones de navegación: `Images` (`PropertyImage`), `Offers`, y `Chats`.

2. **`Improvement.cs`**:
   - Campos: `Name`, `Description`.
   - Relación muchos-a-muchos implícita: `ICollection<Property> Properties`.

3. **`PropertyImage.cs`**:
   - Campos: `ImageUrl`, `PropertyId`, y navegación a `Property`.

4. **`PropertyType.cs`** y **`SaleType.cs`**:
   - Campos: `Name`, `Description`. Navegación a `ICollection<Property> Properties`.

5. **`Offer.cs`**:
   - Campos: `MontoOfertado`, `Status` (Enum `OfferStatus`), `ClienteId` (string), `FechaOferta` (DateTime), `PropertyId`, y navegación a `Property`.

6. **`Chat.cs`**:
   - Hilo de mensajes. Campos: `ClienteId` (string), `AgenteId` (string), `PropertyId`, `MessageContent`, `SenderId` y `SentAt`.

7. **`MortgageSimulation.cs`**:
   - Campos: `MontoInicialAportado`, `TasaInteresAnual`, `PropertyId`, `ClienteId`.

#### B. Enums e Infraestructura Común
- **`Roles.cs`**: Enum con los roles iniciales (`Admin`, `Agent`, `Client`, `Developer`).
- **`OfferStatus.cs`**: Enum con estados (`Pending`, `Accepted`, `Rejected`).
- **`AuditableBaseEntity.cs`**: Clase abstracta base con `Id` (int), `CreatedBy`, `Created`, `LastModifiedBy` y `LastModified`.

---

### Capa 2.2: RealEstateApp.Core.Application (Inicial)

- **Interfaces de Repositorio**:
  - `IGenericRepository<T>`: Métodos CRUD genéricos: `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetAllAsync`, `GetByIdAsync`.
  - `IPropertyRepository`: Heredaba de `IGenericRepository<Property>` sin métodos propios iniciales.
- **Interfaces de Servicios**:
  - `IEmailService`: Firma `SendAsync(string to, string subject, string body)`.
  - `IFileStorageService`: Firmas `UploadFileAsync` y `DeleteFileAsync` para manejo de archivos/imágenes.
  - `IFinancingService`: Firma `GenerateAmortizationSchedule` (Sistema francés).
- **ViewModels**:
  - `MortgageSimulationViewModel.cs`: Modelo de datos para validaciones del formulario de simulación.
- **`ServiceRegistration.cs`**: Solo registraba AutoMapper mediante reflexión (`services.AddAutoMapper(...)`).

---

### Capa 2.3: RealEstateApp.Infrastructure.Persistence (Inicial)

- **`ApplicationDbContext.cs`**:
  - Heredaba de `IdentityDbContext` de forma básica.
  - Conjuntos de datos (`DbSet`) para todas las entidades iniciales de dominio.
  - En `OnModelCreating`, configuraba relaciones básicas de clave foránea en cascada para `Property ↔ PropertyType`, `Property ↔ SaleType`, `PropertyImage ↔ Property`, `Offer ↔ Property` y `Chat ↔ Property`.
  - No incluía mapeos para la tabla intermedia muchos-a-muchos de mejoras, ni soporte de NetTopologySuite ni configuraciones para Identity.
- **`GenericRepository.cs`**:
  - Implementación básica de `IGenericRepository<T>` usando Entity Framework Core y `SaveChangesAsync()`.
- **`ServiceRegistration.cs`**:
  - Registraba el DbContext usando base de datos en memoria (`UseInMemoryDatabase`) o SqlServer según la configuración, pero **no configuraba** ASP.NET Core Identity (Roles, Users ni Managers).

---

### Capa 2.4: RealEstateApp.Infrastructure.Shared (Inicial)

- Contenía mockups/simuladores de infraestructura:
  - **`EmailService.cs`**: Simulaba el envío escribiendo un mensaje en la consola de comandos.
  - **`FileStorageService.cs`**: Generaba URLs ficticias en la nube (`https://cloud-storage.realestateapp.com/...`) sin realizar operaciones en disco o almacenamiento real.
- **`ServiceRegistration.cs`**: Registraba `EmailService` y `FileStorageService` como servicios `Transient`.

---

### Capa 2.5: Presentación (Inicial)

1. **`RealEstateApp.Presentation.WebApp`**:
   - Proyecto ASP.NET Core MVC limpio.
   - Su archivo `Program.cs` no tenía referencias al DbContext, ni al pipeline de autenticación (Identity) ni inyecciones de los servicios de Application o Persistence.
   - Las vistas eran las predeterminadas de la plantilla MVC (Bienvenida básica con Bootstrap).

2. **`RealEstateApp.Presentation.WebApi`**:
   - Proyecto Web API básico.
   - Su archivo `Program.cs` contenía únicamente el endpoint demo autogenerado `WeatherForecast`.
   - No tenía referencias de proyecto a `RealEstateApp.Core.Application`, por lo que no podía utilizar DTOs ni interfaces de negocio.
