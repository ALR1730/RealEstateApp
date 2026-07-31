# 🛠️ Ficha Técnica de Correcciones y Parches — RealEstateApp

Este documento registra de manera acumulativa y detallada cada corrección de vulnerabilidad, bug funcional, refactorización y mejora de infraestructura realizada en la solución **RealEstateApp**.

---

## 📅 Registro de Cambios y Correcciones

### 🔐 Corrección 1.1 — Eliminación de Credenciales Google OAuth Hardcodeadas

- **Severidad**: 🔴 Crítico (Seguridad)
- **Componente**: `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/appsettings.json`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/RealEstateApp.Presentation.WebApp.csproj`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs`
- **Detalles Técnicos de la Solución**:
  1. Se eliminaron del archivo `appsettings.json` los valores en texto plano de `GoogleAuth:ClientId` y `GoogleAuth:ClientSecret`, reemplazándolos por cadenas vacías `""`.
  2. Se habilitó `dotnet user-secrets` en la WebApp generando el `UserSecretsId` `819ef8c1-6685-4d0a-9719-4b8949d5cac8` en el `.csproj`.
  3. Se almacenaron de forma segura las credenciales reales en el almacén de secretos local del desarrollador (`dotnet user-secrets set`).
  4. En `Program.cs`, se modificó la registración del middleware `AddGoogle()` para evaluar si `googleClientId` y `googleClientSecret` están presentes en la configuración antes de invocar `AddGoogle()`. Si no están configurados, el sistema emite una advertencia en consola y continúa iniciando limpiamente sin fallar por credenciales DUMMY.

---

### 🔑 Corrección 1.2 — Protección de Clave de Firma de Tokens JWT

- **Severidad**: 🔴 Crítico (Seguridad)
- **Componentes**: `RealEstateApp.Presentation.WebApi`, `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.json`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/RealEstateApp.Presentation.WebApi.csproj`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
  - Secretos locales en `RealEstateApp.Presentation.WebApp`
- **Detalles Técnicos de la Solución**:
  1. Se eliminó la clave secreta `JWTSettings:Key` en texto plano del `appsettings.json` de la Web API.
  2. Se configuró `dotnet user-secrets` en la Web API generando el `UserSecretsId` `75f5011c-8493-4e5e-86fc-deca8ac9083a`.
  3. Se guardó la clave de firma JWT en `user-secrets` de la Web API y en los `user-secrets` de la WebApp (necesario para la emisión de tokens en el Panel de Desarrolladores).
  4. En `WebApi/Program.cs`, se eliminó el valor por defecto inseguro (`"DefaultSecretKey1234567890123456"`) y se agregó una validación estricta al inicio: si `JWTSettings:Key` está vacío o no está configurado, el sistema lanza una `InvalidOperationException` impidiendo que la API levante con claves inseguras o nulas.

---

### 🌐 Corrección 1.3 — Restricción de Política CORS en Web API

- **Severidad**: 🔴 Crítico (Seguridad)
- **Componente**: `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.json`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
- **Detalles Técnicos de la Solución**:
  1. Se eliminó la política permisiva `AllowAllCors` que utilizaba `policy.AllowAnyOrigin()`.
  2. Se definió la sección `AllowedOrigins` en `appsettings.json` especificando los dominios autorizados de desarrollo (`http://localhost:5000`, `https://localhost:5001`, `http://localhost:5196`, `https://localhost:7196`).
  3. En `Program.cs`, se configuró la nueva política `AllowSpecificOrigins` que lee dinámicamente el arreglo de orígenes desde configuración, aplicando `.WithOrigins(...)`, `.AllowAnyHeader()`, `.AllowAnyMethod()` y `.AllowCredentials()`.
  4. Se actualizó el middleware de pipeline `app.UseCors("AllowSpecificOrigins")`.

---

### 💬 Corrección 1.4 — Autenticación y Validación HMAC en Webhook de WhatsApp

- **Severidad**: 🔴 Crítico (Seguridad)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/DTOs/WhatsApp/WhatsAppSettings.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/WhatsAppWebhookController.cs`
- **Detalles Técnicos de la Solución**:
  1. Se agregó la propiedad `AppSecret` a la clase `WhatsAppSettings`.
  2. En `WhatsAppWebhookController.cs`, se implementó la lectura del cuerpo sin procesar (`StreamReader`) del request POST.
  3. Se agregó la función `IsValidHmacSignature` que calcula el hash **HMAC SHA-256** utilizando la `AppSecret` configurada y compara la firma enviada por Meta en la cabecera `X-Hub-Signature-256` utilizando `CryptographicOperations.FixedTimeEquals` para prevenir ataques de temporización (timing attacks).
  4. En ambientes que no sean de desarrollo, la API rechaza con `401 Unauthorized` cualquier petición POST sin firma o con firma inválida.

---

### 🛡️ Corrección 1.5 — Remoción de Bypass CSRF y Encriptación de Tokens AntiForgery en AJAX

- **Severidad**: 🔴 Crítico (Seguridad)
- **Componente**: `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AccountController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Views/Account/DeveloperPanel.cshtml`
- **Detalles Técnicos de la Solución**:
  1. Se eliminó el atributo `[IgnoreAntiforgeryToken]` del método `GenerateJwtToken` en `AccountController.cs` y se reemplazó por el atributo de validación de seguridad `[ValidateAntiForgeryToken]`.
  2. En `Program.cs` de la WebApp, se configuró la cabecera del servicio Antiforgery `AddAntiforgery(options => options.HeaderName = "RequestVerificationToken")`.
  3. En `DeveloperPanel.cshtml`, se inyectó `@Html.AntiForgeryToken()` dentro del formulario y se modificó la llamada `$.ajax` agregando la cabecera `RequestVerificationToken` extraída dinámicamente de la vista.

---

### 📄 Corrección 3.1 — Persistencia y Visualización de Cartas de Pre-Aprobación Bancaria

- **Severidad**: 🔴 Crítico (Bug Funcional)
- **Componentes**: `RealEstateApp.Core.Domain`, `RealEstateApp.Core.Application`, `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Domain/Entities/Offer.cs`
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Offer/SaveOfferViewModel.cs`
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Offer/OfferViewModel.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/OffersController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Views/Agent/Offers.cshtml`
- **Detalles Técnicos de la Solución**:
  1. Se agregó el atributo de persistencia `PreApprovalLetterUrl` a la entidad de dominio `Offer`.
  2. Se expuso `PreApprovalLetterUrl` en `SaveOfferViewModel` y `OfferViewModel` para el binding y la visualización de la vista.
  3. En `OffersController.cs`, tras subir el archivo PDF/imagen mediante `_fileStorageService.UploadFileAsync`, se asignó la URL generada en `vm.PreApprovalLetterUrl = letterUrl`.
  4. En la vista del Agente (`Agent/Offers.cshtml`), se agregó la columna **Carta Bancaria** con el botón **"Ver Carta"** (`<a href="@offer.PreApprovalLetterUrl" target="_blank">`) permitiendo la consulta directa del documento por parte del agente.

---

### 💬 Corrección 3.2 — Integridad Referencial y Soporte de Canales de Soporte en Chat (PropertyId Nullable)

- **Severidad**: 🔴 Crítico (Bug Funcional / Base de Datos)
- **Componentes**: `RealEstateApp.Core.Domain`, `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Domain/Entities/Chat.cs`
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Chat/ChatViewModel.cs`
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Chat/SaveChatViewModel.cs`
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Repositories/IChatRepository.cs`
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Services/IChatService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/ChatService.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/ChatRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Contexts/ApplicationDbContext.cs`
  - `src/Core/RealEstateApp.Core.Application/Mappings/GeneralProfile.cs`
- **Detalles Técnicos de la Solución**:
  1. Se modificó la clave foránea `PropertyId` a tipo nulo (`int? PropertyId`) en las entidades `Chat`, `ChatViewModel` y `SaveChatViewModel`.
  2. En `ApplicationDbContext.cs`, se configuró la relación `.IsRequired(false)` para `PropertyId` manteniendo la eliminación en cascada para propiedades existentes.
  3. En `ChatRepository.cs` y `ChatService.cs`, se mapearon las peticiones de canales de soporte (PropertyId <= 0) para que persistan `PropertyId = NULL` en la base de datos SQL Server, eliminando las fallas de violaciones de clave foránea `FK_Chats_Properties_PropertyId`.
  4. Se ajustó el perfil de AutoMapper (`GeneralProfile.cs`) para soportar la conversión segura de `PropertyId` nulo sin lanzar excepciones.

---

### ⚡ Corrección 4.1 — Filtrado Directo en Base de Datos (IQueryable vs Enumerable RAM)

- **Severidad**: 🔴 Crítico (Rendimiento)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Repositories/IPropertyRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/PropertyRepository.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`
- **Detalles Técnicos de la Solución**:
  1. Se declararon e implementaron los métodos `GetWithFiltersAsync`, `GetByAgentIdAsync` y `GetByCodeAsync` en la capa de persistencia (`PropertyRepository.cs`).
  2. Las consultas ahora construyen expresiones `IQueryable<Property>` dinámicas utilizando `.Where(...)` de Entity Framework Core, traduciendo directamente los filtros a cláusulas SQL nativas `WHERE Code = @p0`, `WHERE Price >= @p0 AND Price <= @p1`, `WHERE AgentId = @p0`, etc.
  3. En `PropertyService.cs`, se eliminaron las llamadas a `_propertyRepository.GetAllAsync()` seguidas de `.AsEnumerable()`, eliminando por completo la carga innecesaria de todo el catálogo de inmuebles en la memoria RAM del servidor web.

---

### 🛡️ Corrección 1.6 — Rate Limiting en Endpoints Sensibles y Webhooks

- **Severidad**: 🟠 Alta (Seguridad / Anti-Bruteforce & Anti-DDoS)
- **Componentes**: `RealEstateApp.Presentation.WebApp`, `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AccountController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/WhatsAppWebhookController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AccountController.cs`
- **Detalles Técnicos de la Solución**:
  1. Se registró el middleware nativo `Microsoft.AspNetCore.RateLimiting` en `WebApp/Program.cs` y `WebApi/Program.cs`.
  2. Se definió la política `AuthPolicy` con una ventana fija de 1 minuto y máximo 5 solicitudes (`PermitLimit = 5`, `QueueLimit = 0`), retornando `429 Too Many Requests` ante excesos.
  3. Se definió la política `WebhookPolicy` con un límite de 60 solicitudes por minuto para proteger el webhook de recepción de mensajes.
  4. Se decoraron las acciones sensibles (`Login` POST, `Register` POST, `GenerateJwtToken` POST, `AuthenticateAsync` POST y `WhatsAppWebhookController`) con `[EnableRateLimiting]`.

---

### 🛡️ Corrección 1.7 — Sanitización contra Stored XSS en Mensajería de Chat

- **Severidad**: 🟠 Alta (Seguridad / Stored Cross-Site Scripting)
- **Componentes**: `RealEstateApp.Core.Application`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Services/ChatService.cs`
- **Detalles Técnicos de la Solución**:
  1. En `ChatService.cs` (método `SendMessage`), se aplicó la codificación previa a la persistencia en base de datos utilizando `System.Net.WebUtility.HtmlEncode(vm.MessageContent)`.
  2. Todo contenido inyectado por clientes o usuarios (`<script>`, `<img src=x onerror=...>`, etc.) es convertido en su representación de entidad de texto HTML seguro (`&lt;script&gt;`), previniendo su ejecución en navegadores o clientes Web/API.

---

### 🔑 Corrección 1.8 — Fortalecimiento de la Política de Contraseñas (OWASP 8+ Caracteres)

- **Severidad**: 🟠 Alta (Seguridad / Autenticación)
- **Componentes**: `RealEstateApp.Infrastructure.Persistence`, `RealEstateApp.Core.Application`
- **Archivos Modificados**:
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/ServiceRegistration.cs`
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Account/RegisterViewModel.cs`
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Account/EditProfileViewModel.cs`
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Account/EditDeveloperViewModel.cs`
- **Detalles Técnicos de la Solución**:
  1. En `ServiceRegistration.cs` (sección `AddIdentity`), se actualizó `options.Password.RequiredLength = 8;` cumpliendo el estándar OWASP de longitud mínima requerida.
  2. Se actualizaron las restricciones de DataAnnotation `[StringLength(100, MinimumLength = 8)]` y los mensajes de validación en todos los ViewModels de registro y edición de cuenta.

---

### 🧅 Corrección 2.1 — Desacoplamiento de Identity en la Capa de Aplicación (Onion Architecture)

- **Severidad**: 🟠 Alta (Arquitectura / Reglas de Dominio)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/DTOs/Account/AccountUserDto.cs`
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Services/IAccountService.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/AgentService.cs`
- **Detalles Técnicos de la Solución**:
  1. Se creó el DTO abstracto `AccountUserDto` en la capa `Core.Application` para transferir datos de usuario sin depender del espacio de nombres `Microsoft.AspNetCore.Identity`.
  2. Se expandió la interfaz `IAccountService` con los métodos `GetUsersInRoleAsync`, `GetUserByIdAsync` y `DeleteUserAsync`, implementándolos en `Infrastructure.Persistence/Services/AccountService.cs`.
  3. Se refactorizó `AgentService.cs` para remover la inyección directa de `UserManager<IdentityUser>` y la directiva `using Microsoft.AspNetCore.Identity;`, restaurando la independencia de la capa `Core.Application` respecto a detalles de infraestructura.

---

### ⚡ Corrección 2.2 — Optimización de Persistencia en Lote (Eliminación del Problema N+1 SaveChanges)

- **Severidad**: 🟠 Alta (Arquitectura / Rendimiento BD)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Repositories/IGenericRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/GenericRepository.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/AgentService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`
- **Detalles Técnicos de la Solución**:
  1. Se agregaron las operaciones en lote `AddRangeAsync`, `UpdateRangeAsync` y `DeleteRangeAsync` a la interfaz genérica `IGenericRepository<T>` y su implementación base `GenericRepository<T>`.
  2. En `AgentService.DeleteAgentCascadeAsync`, se reemplazaron las iteraciones con llamadas individuales a `DeleteAsync` por llamadas agrupadas en lote a `DeleteRangeAsync` para imágenes, favoritos, ofertas, chats y propiedades.
  3. En `PropertyService.Add`, se agruparon las imágenes subidas en una lista `imageEntities` y se registraron mediante un único `AddRangeAsync`, reduciendo drásticamente las peticiones repetitivas a la base de datos SQL Server.

---

### ⚛️ Corrección 2.3 — Transacciones Explícitas y Operaciones Atómicas (`AcceptOffer`)

- **Severidad**: 🟠 Alta (Arquitectura / Consistencia de Datos)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Repositories/IOfferRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/OfferService.cs`
- **Detalles Técnicos de la Solución**:
  1. Se declaró `AcceptOfferTransactionAsync(int offerId)` en `IOfferRepository.cs` e implementó en `OfferRepository.cs`.
  2. Se envolvió toda la regla de negocio (marcar oferta seleccionada como `Accepted`, cambiar estado de la propiedad a `"Vendida"`, y rechazar en lote el resto de ofertas como `Rejected`) dentro de una transacción explícita de Entity Framework Core (`using var transaction = await _dbContext.Database.BeginTransactionAsync()`).
  3. Ante cualquier fallo en los 3 pasos, se invoca `RollbackAsync()` previniendo estados inconsistentes en la base de datos (por ejemplo, ofertas aceptadas con inmuebles que permanecen como "Disponibles").

---

### 🛡️ Corrección 1.9 — Protección de Seeds de Usuarios de Prueba en Producción

- **Severidad**: 🟡 Media (Seguridad / Autenticación)
- **Componentes**: `RealEstateApp.Presentation.WebApp`, `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
- **Detalles Técnicos de la Solución**:
  1. En `WebApp/Program.cs` y `WebApi/Program.cs`, se envolvieron las ejecuciones de los seeds de demostración (`DefaultAdminUser`, `DefaultAgentUser`, `DefaultClientUser`, `DefaultDeveloperUser` y `DefaultRealEstateData`) dentro del guard `if (app.Environment.IsDevelopment())`.
  2. Los roles esenciales del sistema (`DefaultRoles.SeedAsync`) se mantienen ejecutándose en todos los ambientes para garantizar el funcionamiento inicial sin comprometer cuentas ni contraseñas de prueba en producción.

---

### 🛡️ Corrección 1.10 — Restricción de Swagger UI a Ambiente de Desarrollo (`WebApi`)

- **Severidad**: 🟡 Media (Seguridad / Exposición de Información)
- **Componentes**: `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
- **Detalles Técnicos de la Solución**:
  1. Se envolvió la llamada a los middlewares `app.UseSwagger()` y `app.UseSwaggerUI(...)` dentro del condicional `if (app.Environment.IsDevelopment())`.
  2. Esto previene que en ambientes de producción se exponga la especificación OpenAPI (`/swagger/v1/swagger.json`) e interfaz de usuario interactiva, mitigando intentos de reconocimiento y mapeo de endpoints.

---

### 🧹 Corrección 2.4 — Limpieza de Archivos Residuales `Class1.cs`

- **Severidad**: 🟡 Media (Limpieza de Código / Mantenimiento)
- **Componentes**: `RealEstateApp.Core.Domain`, `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`, `RealEstateApp.Infrastructure.Shared`
- **Archivos Eliminados**:
  - `src/Core/RealEstateApp.Core.Domain/Class1.cs`
  - `src/Core/RealEstateApp.Core.Application/Class1.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Class1.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Shared/Class1.cs`
- **Detalles Técnicos de la Solución**:
  1. Se eliminaron los 4 archivos `Class1.cs` autogenerados durante la creación inicial de las bibliotecas de clases de .NET, asegurando un proyecto limpio sin clases vacías ni código residual.

---

### 👤 Corrección 3.3 — Lectura y Mapeo de Nombres y Apellidos de Agentes desde User Claims

- **Severidad**: 🟠 Alta (Bug Funcional / UI)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/DTOs/Account/AccountUserDto.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/AgentService.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Seeds/DefaultAgentUser.cs`
- **Detalles Técnicos de la Solución**:
  1. Se agregaron las propiedades `FirstName`, `LastName` y `ProfilePictureUrl` al DTO abstracto `AccountUserDto.cs`.
  2. En `AccountService.cs` (`GetUsersInRoleAsync` y `GetUserByIdAsync`), se leen los claims `"FirstName"`, `"LastName"` y `"ProfilePicture"` del usuario registrado y se mapean al DTO.
  3. En `AgentService.cs`, se actualizaron los mapeos para asignar `FirstName = user.FirstName` y `LastName = user.LastName`.
  4. En `DefaultAgentUser.cs`, se aseguraron los claims de nombre y apellido para los agentes de prueba de la solución, resolviendo el problema de despliegue donde los agentes mostraban su username con apellido vacío.

---

### ✉️ Corrección 3.4 — Parametrización de Rutas de Confirmación de Correo Electrónico

- **Severidad**: 🟠 Alta (Bug Funcional / Enlaces de Activación)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`, `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Services/IAccountService.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AccountController.cs`
- **Detalles Técnicos de la Solución**:
  1. Se añadió el parámetro opcional `route` a la firma `RegisterUserAsync`.
  2. En `AccountService.cs`, se implementó la resolución dinámica del target URI: si no se especifica ruta, se verifica si el origen proviene de Web API o de WebApp MVC.
  3. En `WebApp/AccountController.cs`, se pasa explícitamente `"Account/ConfirmEmail"`, garantizando que al hacer clic en el correo de confirmación enviado a clientes registrados en la WebApp se abra la vista HTML `ConfirmEmailResult.cshtml`.

---

### 🔢 Corrección 3.5 — Validación de Unicidad en la Generación del Código de Propiedades

- **Severidad**: 🟠 Alta (Bug Funcional / Integridad BD)
- **Componentes**: `RealEstateApp.Core.Application`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`
- **Detalles Técnicos de la Solución**:
  1. Se reemplazó el método síncrono estático `GenerateUniqueCode()` por la función asíncrona `GenerateUniqueCodeAsync()`.
  2. Se integró una verificación contra la base de datos `await _propertyRepository.GetByCodeAsync(code)` en un ciclo `do-while` para asegurar que cada código alfanumérico autogenerado de 6 caracteres sea único antes de asignarlo al inmueble.
  3. Esto elimina potenciales colisiones en el índice `UNIQUE` de la columna `Code` en SQL Server.

---

### 📝 Corrección 3.6 — Inyección de Usuario Autenticado en Campos de Auditoría del DbContext

- **Severidad**: 🟡 Media (Bug Funcional / Auditoría)
- **Componentes**: `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Contexts/ApplicationDbContext.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/ServiceRegistration.cs`
- **Detalles Técnicos de la Solución**:
  1. Se registró `IHttpContextAccessor` en `ServiceRegistration.cs` y se inyectó de forma opcional en `ApplicationDbContext.cs`.
  2. En `SaveChangesAsync`, se extrae el nombre o identificador del usuario autenticado actual desde `_httpContextAccessor.HttpContext.User`.
  3. Se reemplazó el valor fijo `"System"` por el nombre de usuario dinámico en los campos `CreatedBy` y `LastModifiedBy` de las entidades auditables (`AuditableBaseEntity`), manteniendo `"System"` únicamente como fallback seguro para ejecuciones en segundo plano o seeds.

---

### 🛡️ Corrección 3.7 — Validación de Pertenencia del Agente en la Eliminación de Propiedades

- **Severidad**: 🟡 Media (Bug Funcional / Seguridad de Autorización)
- **Componentes**: `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AgentController.cs`
- **Detalles Técnicos de la Solución**:
  1. En el método HTTP POST `DeleteProperty(int id)` de `AgentController.cs`, se consulta la propiedad `GetByIdSaveViewModel(id)` antes de proceder con el borrado.
  2. Se verifica que el identificador del usuario autenticado coincida con `existing.AgentId`.
  3. En caso de discrepancia o propiedad inexistente, la solicitud se rechaza y se notifica al usuario con un mensaje de error mediante `TempData["ErrorMessage"]`, impidiendo que un agente pueda eliminar inmuebles asignados a otro agente.

---

### ⚡ Corrección 4.2 — Optimización de Eliminación en Cascada del Agente (`DeleteAgentCascadeAsync`)

- **Severidad**: 🔴 Crítica / 🟠 Alta (Rendimiento / Múltiples Consultas N+1)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Repositories/IFavoriteRepository.cs`
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Repositories/IChatRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/FavoriteRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/ChatRepository.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/AgentService.cs`
- **Detalles Técnicos de la Solución**:
  1. Se implementaron los métodos `GetByPropertyIdAsync` en `FavoriteRepository` y `ChatRepository`, así como `GetByUserIdAsync` en `ChatRepository`, ejecutando consultas SQL filtradas directamente en la base de datos.
  2. Se refactorizó `DeleteAgentCascadeAsync` en `AgentService.cs` eliminando por completo las llamadas masivas `GetAllAsync()` en RAM.
  3. Se combinó la recuperación directa filtrada en BD con la eliminación en lote `DeleteRangeAsync`, eliminando cientos de queries individuales N+1 y llamadas repetitivas a `SaveChangesAsync`.

---

### 📄 Corrección 4.3 — Soporte de Paginación en Consultas de Listado de Propiedades

- **Severidad**: 🟠 Alta (Rendimiento / Carga Masiva)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/ViewModels/Property/PropertyFilterViewModel.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/PropertyRepository.cs`
- **Detalles Técnicos de la Solución**:
  1. Se añadieron las propiedades opcionales `PageNumber` y `PageSize` al ViewModel de filtrado de propiedades `PropertyFilterViewModel.cs`.
  2. En `PropertyRepository.cs` (`GetWithFiltersAsync`), se evaluaron `PageNumber` y `PageSize` para aplicar cláusulas de paginación `.Skip((pageNumber - 1) * pageSize).Take(pageSize)` a nivel de consulta EF Core (`IQueryable`), limitando los datos extraídos de SQL Server.

---

### 🏷️ Corrección 4.4 — Consulta Filtrada de Ofertas del Agente (`AgentController.Offers`)

- **Severidad**: 🟡 Media (Rendimiento / Carga Masiva)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`, `RealEstateApp.Presentation.WebApp`
- **Archivos Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Repositories/IOfferRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs`
  - `src/Core/RealEstateApp.Core.Application/Interfaces/Services/IOfferService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/OfferService.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AgentController.cs`
- **Detalles Técnicos de la Solución**:
  1. Se implementó `GetByPropertyIdsAsync(IEnumerable<int> propertyIds)` en `OfferRepository.cs` y `OfferService.cs`.
  2. Se actualizó `AgentController.Offers()` sustituyendo la invocación a `_offerService.GetAllViewModel()` por `_offerService.GetByPropertyIds(agentPropertyIds)`.
  3. Las ofertas se filtran directamente en la base de datos con `WHERE PropertyId IN (...)`, reduciendo la transferencia de datos y eliminando el filtrado en memoria C#.

---

### 🌐 Corrección 5.1 — Restricción de Encabezados de Host (`AllowedHosts`)

- **Severidad**: 🟠 Alta (Configuración de Seguridad / Host Header Injection)
- **Componentes**: `RealEstateApp.Presentation.WebApp`, `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/appsettings.json`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/appsettings.Development.json`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.json`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.Development.json`
- **Detalles Técnicos de la Solución**:
  1. Se actualizó `appsettings.json` reemplazando la configuración permisiva `AllowedHosts: "*"` por listas explícitas de dominios autorizados (`localhost;127.0.0.1;realestateapp.com` para WebApp y `api.realestateapp.com` para WebApi).
  2. Se añadió la propiedad `AllowedHosts: "*"` exclusivamente en los archivos de entorno de desarrollo local (`appsettings.Development.json`).

---

## 📊 Estado Actual del Plan de Correcciones

| ID | Tipo | Descripción | Estado |
|---|---|---|---|
| **1.1** | 🔒 Seguridad | Credenciales Google OAuth en `appsettings.json` | ✅ SOLUCIONADO |
| **1.2** | 🔒 Seguridad | Clave JWT Signing Key hardcodeada y fallback inseguro | ✅ SOLUCIONADO |
| **1.3** | 🔒 Seguridad | Política CORS `AllowAnyOrigin()` | ✅ SOLUCIONADO |
| **1.4** | 🔒 Seguridad | Webhook WhatsApp sin autenticación / firma Meta | ✅ SOLUCIONADO |
| **1.5** | 🔒 Seguridad | CSRF Bypass en `GenerateJwtToken` | ✅ SOLUCIONADO |
| **1.6** | 🔒 Seguridad | Ausencia de Rate Limiting en endpoints de autenticación | ✅ SOLUCIONADO |
| **1.7** | 🔒 Seguridad | Sin sanitización HTML/XSS en mensajes de Chat | ✅ SOLUCIONADO |
| **1.8** | 🔒 Seguridad | Requisito de contraseña débil (6 caracteres) | ✅ SOLUCIONADO |
| **1.9** | 🔒 Seguridad | Guard de ambiente en seeds de usuarios de prueba | ✅ SOLUCIONADO |
| **1.10** | 🔒 Seguridad | Restricción de Swagger UI a ambiente de desarrollo | ✅ SOLUCIONADO |
| **2.1** | 🏗️ Arquitectura | Violación Onion Architecture: Identity en Application Layer | ✅ SOLUCIONADO |
| **2.2** | 🏗️ Arquitectura | N+1 `SaveChangesAsync` en `GenericRepository` | ✅ SOLUCIONADO |
| **2.3** | 🏗️ Arquitectura | Falta de transacciones explícitas en `AcceptOffer` | ✅ SOLUCIONADO |
| **2.4** | 🏗️ Arquitectura | Archivos `Class1.cs` placeholder residuales | ✅ SOLUCIONADO |
| **3.1** | 🐛 Bug | Carta de Pre-aprobación bancaria subida pero no guardada | ✅ SOLUCIONADO |
| **3.2** | 🐛 Bug | Chats de soporte usan PropertyId (0 y -1) sin validar FK | ✅ SOLUCIONADO |
| **3.3** | 🐛 Bug | Nombres de agentes usan UserName y apellido vacío | ✅ SOLUCIONADO |
| **3.4** | 🐛 Bug | Ruta de confirmación email apunta a API en lugar de WebApp | ✅ SOLUCIONADO |
| **3.5** | 🐛 Bug | Posibles colisiones en autogeneración de código de propiedad | ✅ SOLUCIONADO |
| **3.6** | 🐛 Bug | Campos de auditoría `CreatedBy`/`LastModifiedBy` siempre "System" | ✅ SOLUCIONADO |
| **3.7** | 🐛 Bug | `DeleteProperty` de agente no valida pertenencia del inmueble | ✅ SOLUCIONADO |
| **4.1** | ⚡ Rendimiento | `GetAllWithFilters` carga todas las propiedades en RAM | ✅ SOLUCIONADO |
| **4.2** | ⚡ Rendimiento | `DeleteAgentCascade` ejecuta cientos de queries individuales | ✅ SOLUCIONADO |
| **4.3** | ⚡ Rendimiento | Sin paginación en listado de propiedades | ✅ SOLUCIONADO |
| **4.4** | ⚡ Rendimiento | `AgentController.Offers` carga todas las ofertas del sistema | ✅ SOLUCIONADO |
---

### 🔒 Corrección 5.2 — Configuración de Encabezados HSTS con Preload y Subdominios

- **Severidad**: 🟡 Media (Configuración de Seguridad / HSTS)
- **Componentes**: `RealEstateApp.Presentation.WebApp`, `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
- **Detalles Técnicos de la Solución**:
  1. Se configuró `AddHsts` en el contenedor de servicios de ambos proyectos estableciendo `Preload = true`, `IncludeSubDomains = true` y `MaxAge = 365 días`.
  2. Se invocó `app.UseHsts()` en la tubería de middleware para entornos de producción, mitigando ataques de Man-in-the-Middle y SSL Stripping.

---

### 🔐 Corrección 5.3 — Redirección HTTPS en Web API RESTful

- **Severidad**: 🟡 Media (Configuración de Seguridad / Redirección Cifrada)
- **Componentes**: `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
- **Detalles Técnicos de la Solución**:
  1. Se agregó `app.UseHttpsRedirection()` en la tubería HTTP de `WebApi/Program.cs`.
  2. Esto garantiza que cualquier petición realizada a endpoints HTTP no cifrados sea inmediatamente redireccionada hacia el esquema seguro HTTPS.

---

### 🧹 Corrección 6.1 — Centralización de Estados de Propiedad (`PropertyStatus`)

- **Severidad**: 🟡 Media (Deuda Técnica / Cadenas Mágicas)
- **Componentes**: `RealEstateApp.Core.Domain`, `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`, `RealEstateApp.Presentation.WebApp`, `RealEstateApp.Presentation.WebApi`
- **Archivos Creados/Modificados**:
  - `src/Core/RealEstateApp.Core.Domain/Constants/PropertyStatus.cs` [NUEVO]
  - `src/Core/RealEstateApp.Core.Domain/Entities/Property.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/OfferService.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/FavoriteRepository.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Contexts/ApplicationDbContext.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Seeds/DefaultRealEstateData.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AdminController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/HomeController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/OffersController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/PropertiesController.cs`
- **Detalles Técnicos de la Solución**:
  1. Se creó la clase de constantes `PropertyStatus` con los valores `Available = "Disponible"`, `Reserved = "Reservada"`, y `Sold = "Vendida"`.
  2. Se reemplazaron todas las cadenas mágicas literales por referencias fuertemente tipadas a `PropertyStatus`, evitando fallos silenciosos por errores ortográficos y asegurando consistencia.

---

### 🧩 Corrección 6.2 — Reutilización de Lógica de Usuario Activo (`IdentityExtensions`)

- **Severidad**: 🟡 Media (Deuda Técnica / Duplicación de Código)
- **Componentes**: `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`, `RealEstateApp.Presentation.WebApp`
- **Archivos Creados/Modificados**:
  - `src/Core/RealEstateApp.Core.Application/Extensions/IdentityExtensions.cs` [NUEVO]
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AccountController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AdminController.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Views/Admin/Developers.cshtml`
- **Detalles Técnicos de la Solución**:
  1. Se creó la clase estática `IdentityExtensions` en `RealEstateApp.Core.Application.Extensions` con el método `IsActiveUser(this IdentityUser user)`.
  2. Se sustituyó la evaluación repetitiva de `!user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow` en servicios, controladores y vistas por la llamada limpia al método de extensión `user.IsActiveUser()`.

---

### 🚨 Corrección 6.3 — Excepciones Tipadas de Dominio (`DomainException`, `NotFoundException`, `ValidationException`)

- **Severidad**: 🟡 Media (Deuda Técnica / Manejo de Excepciones)
- **Componentes**: `RealEstateApp.Core.Domain`, `RealEstateApp.Core.Application`, `RealEstateApp.Infrastructure.Persistence`
- **Archivos Creados/Modificados**:
  - `src/Core/RealEstateApp.Core.Domain/Exceptions/DomainException.cs` [NUEVO]
  - `src/Core/RealEstateApp.Core.Domain/Exceptions/NotFoundException.cs` [NUEVO]
  - `src/Core/RealEstateApp.Core.Domain/Exceptions/ValidationException.cs` [NUEVO]
  - `src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/OfferService.cs`
  - `src/Core/RealEstateApp.Core.Application/Services/AgentService.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs`
  - `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs`
- **Detalles Técnicos de la Solución**:
  1. Se definieron las excepciones fuertemente tipadas `DomainException`, `NotFoundException` y `ValidationException` en la capa `Core.Domain`.
  2. Se reemplazaron todas las instancias de `throw new Exception(...)` y `throw new System.Exception(...)` por la excepción tipada adecuada (`NotFoundException` o `ValidationException`), permitiendo un manejo estructurado de errores y respuestas HTTP oportunas.

---

### 🌱 Corrección 6.4 — Protección de Seeds de Datos por Entorno de Ejecución

- **Severidad**: 🟢 Baja (Limpieza / Seguridad de Entorno)
- **Componentes**: `RealEstateApp.Presentation.WebApp`, `RealEstateApp.Presentation.WebApi`
- **Archivos Modificados**:
  - `src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs`
  - `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs`
- **Detalles Técnicos de la Solución**:
  1. Se garantizó que la ejecución de los seeders de usuarios y datos de demostración (`DefaultAdminUser`, `DefaultAgentUser`, `DefaultClientUser`, `DefaultDeveloperUser`, `DefaultRealEstateData`) se encuentre protegida bajo la condición `if (app.Environment.IsDevelopment())`.
  2. En producción únicamente se ejecutan las migraciones iniciales y la creación del listado estándar de roles (`DefaultRoles`).

---

## 📊 Estado Actual del Plan de Correcciones

| ID | Tipo | Descripción | Estado |
|---|---|---|---|
| **1.1** | 🔒 Seguridad | Credenciales Google OAuth en `appsettings.json` | ✅ SOLUCIONADO |
| **1.2** | 🔒 Seguridad | Clave JWT Signing Key hardcodeada y fallback inseguro | ✅ SOLUCIONADO |
| **1.3** | 🔒 Seguridad | Política CORS `AllowAnyOrigin()` | ✅ SOLUCIONADO |
| **1.4** | 🔒 Seguridad | Webhook WhatsApp sin autenticación / firma Meta | ✅ SOLUCIONADO |
| **1.5** | 🔒 Seguridad | CSRF Bypass en `GenerateJwtToken` | ✅ SOLUCIONADO |
| **1.6** | 🔒 Seguridad | Ausencia de Rate Limiting en endpoints de autenticación | ✅ SOLUCIONADO |
| **1.7** | 🔒 Seguridad | Sin sanitización HTML/XSS en mensajes de Chat | ✅ SOLUCIONADO |
| **1.8** | 🔒 Seguridad | Requisito de contraseña débil (6 caracteres) | ✅ SOLUCIONADO |
| **1.9** | 🔒 Seguridad | Guard de ambiente en seeds de usuarios de prueba | ✅ SOLUCIONADO |
| **1.10** | 🔒 Seguridad | Restricción de Swagger UI a ambiente de desarrollo | ✅ SOLUCIONADO |
| **2.1** | 🏗️ Arquitectura | Violación Onion Architecture: Identity en Application Layer | ✅ SOLUCIONADO |
| **2.2** | 🏗️ Arquitectura | N+1 `SaveChangesAsync` en `GenericRepository` | ✅ SOLUCIONADO |
| **2.3** | 🏗️ Arquitectura | Falta de transacciones explícitas en `AcceptOffer` | ✅ SOLUCIONADO |
| **2.4** | 🏗️ Arquitectura | Archivos `Class1.cs` placeholder residuales | ✅ SOLUCIONADO |
| **3.1** | 🐛 Bug | Carta de Pre-aprobación bancaria subida pero no guardada | ✅ SOLUCIONADO |
| **3.2** | 🐛 Bug | Chats de soporte usan PropertyId (0 y -1) sin validar FK | ✅ SOLUCIONADO |
| **3.3** | 🐛 Bug | Nombres de agentes usan UserName y apellido vacío | ✅ SOLUCIONADO |
| **3.4** | 🐛 Bug | Ruta de confirmación email apunta a API en lugar de WebApp | ✅ SOLUCIONADO |
| **3.5** | 🐛 Bug | Posibles colisiones en autogeneración de código de propiedad | ✅ SOLUCIONADO |
| **3.6** | 🐛 Bug | Campos de auditoría `CreatedBy`/`LastModifiedBy` siempre "System" | ✅ SOLUCIONADO |
| **3.7** | 🐛 Bug | `DeleteProperty` de agente no valida pertenencia del inmueble | ✅ SOLUCIONADO |
| **4.1** | ⚡ Rendimiento | `GetAllWithFilters` carga todas las propiedades en RAM | ✅ SOLUCIONADO |
| **4.2** | ⚡ Rendimiento | `DeleteAgentCascade` ejecuta cientos de queries individuales | ✅ SOLUCIONADO |
| **4.3** | ⚡ Rendimiento | Sin paginación en listado de propiedades | ✅ SOLUCIONADO |
| **4.4** | ⚡ Rendimiento | `AgentController.Offers` carga todas las ofertas del sistema | ✅ SOLUCIONADO |
| **5.1** | ⚙️ Configuración | `AllowedHosts` permite cualquier host | ✅ SOLUCIONADO |
| **5.2** | ⚙️ Configuración | HTTPS Redirection sin HSTS Preload y Subdominios | ✅ SOLUCIONADO |
| **5.3** | ⚙️ Configuración | WebApi no usa HTTPS Redirection | ✅ SOLUCIONADO |
| **6.1** | 🧹 Deuda Técnica | Strings mágicos para estados de propiedad | ✅ SOLUCIONADO |
| **6.2** | 🧹 Deuda Técnica | Duplicación de lógica de verificación de estado activo | ✅ SOLUCIONADO |
| **6.3** | 🧹 Deuda Técnica | Manejo de excepciones con `throw new Exception()` | ✅ SOLUCIONADO |
| **6.4** | 🧹 Deuda Técnica | Seeds de datos ejecutan en producción | ✅ SOLUCIONADO |

---

## 🧪 Verificación de Compilación

- **Estado de la Solución**: `Build Succeeded`
- **Errores**: 0
- **Advertencias**: 0
