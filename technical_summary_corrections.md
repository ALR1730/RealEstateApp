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
| **2.1** | 🏗️ Arquitectura | Violación Onion Architecture: Identity en Application Layer | ⏳ Pendiente |
| **2.2** | 🏗️ Arquitectura | N+1 `SaveChangesAsync` en `GenericRepository` | ⏳ Pendiente |
| **2.3** | 🏗️ Arquitectura | Falta de transacciones explícitas en `AcceptOffer` | ⏳ Pendiente |
| **3.1** | 🐛 Bug | Carta de Pre-aprobación bancaria subida pero no guardada | ✅ SOLUCIONADO |
| **3.2** | 🐛 Bug | Chats de soporte usan PropertyId (0 y -1) sin validar FK | ✅ SOLUCIONADO |
| **4.1** | ⚡ Rendimiento | `GetAllWithFilters` carga todas las propiedades en RAM | ✅ SOLUCIONADO |

---

## 🧪 Verificación de Compilación

- **Estado de la Solución**: `Build Succeeded`
- **Errores**: 0
- **Advertencias**: 0
