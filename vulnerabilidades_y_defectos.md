# 🔴 Análisis de Vulnerabilidades y Defectos — RealEstateApp

> Auditoría completa del código fuente realizada sobre todas las capas de la arquitectura Onion.
> Cada hallazgo incluye **severidad**, **ubicación exacta en el código** y **solución propuesta**.

---

## Leyenda de Severidad

| Icono | Nivel | Descripción |
|-------|-------|-------------|
| 🔴 | **CRÍTICO** | Vulnerabilidad explotable que compromete la seguridad o integridad de datos |
| 🟠 | **ALTO** | Defecto que causa comportamiento incorrecto o pérdida de datos |
| 🟡 | **MEDIO** | Problema que afecta rendimiento, mantenibilidad o experiencia de usuario |
| 🟢 | **BAJO** | Mejora menor, limpieza de código o best practice |

---

## 🔒 1. Vulnerabilidades de Seguridad

### ✅ ~~1.1 Credenciales de Google OAuth Hardcodeadas en Control de Versiones~~ — SOLUCIONADO

> **Resuelto**: Credenciales movidas a `dotnet user-secrets`. `appsettings.json` ahora tiene valores vacíos. `Program.cs` registra Google Auth condicionalmente solo si las credenciales existen.

**Archivo**: [appsettings.json](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/appsettings.json#L13-L16)

```json
"GoogleAuth": {
    "ClientId": "330513859687-gv6g3mnemu24h9jg2lganl09q8dc93c2.apps.googleusercontent.com",
    "ClientSecret": "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"
}
```

**Problema**: Las credenciales de OAuth están expuestas en texto plano en un archivo que probablemente se encuentra en el repositorio Git. Cualquier persona con acceso al repo puede usar estas credenciales para suplantación de identidad.

**Solución**:
```csharp
// Mover a User Secrets (desarrollo) o variables de entorno (producción)
// Terminal: dotnet user-secrets set "GoogleAuth:ClientId" "tu-client-id"
builder.Configuration.AddUserSecrets<Program>();
// O usar Azure Key Vault / AWS Secrets Manager en producción
```

> [!CAUTION]
> **Acción inmediata**: Revocar las credenciales actuales en Google Cloud Console, regenerar nuevas y almacenarlas en `dotnet user-secrets` o variables de entorno.

---

### ✅ ~~1.2 Clave JWT Signing Key Hardcodeada y Predecible~~ — SOLUCIONADO

> **Resuelto**: Clave removida de `appsettings.json`. Se movió a `dotnet user-secrets` en WebApi y WebApp. En `Program.cs` de WebApi se eliminó el fallback inseguro hardcodeado y se requiere obligatoriamente la configuración.

**Archivo**: [WebApi appsettings.json](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.json#L7)

```json
"Key": "SuperSecretRealEstateAppJwtSigningKey2026SecureKeyWithAtLeast256Bits!"
```

**Problema**: La clave secreta JWT está expuesta en texto plano. Un atacante que obtenga esta clave puede generar tokens válidos para cualquier usuario/rol, incluyendo Admin.

**Además**, en [WebApi Program.cs L96](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs#L96) existe un fallback inseguro:
```csharp
IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    builder.Configuration["JWTSettings:Key"] ?? "DefaultSecretKey1234567890123456"))
```

**Solución**: Usar variables de entorno o un almacén de secretos. Nunca hardcodear claves de firma.

---

### ✅ ~~1.3 Política CORS Permite Todo Origen~~ — SOLUCIONADO

> **Resuelto**: Se reemplazó `AllowAnyOrigin()` y la política `AllowAllCors` por una política restrictiva `AllowSpecificOrigins` que lee los dominios permitidos desde `AllowedOrigins` en `appsettings.json` (o por defecto puertos locales de WebApp y WebApi) y habilita `AllowCredentials()`.

**Archivo**: [WebApi Program.cs L21-L29](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs#L21-L29)

```csharp
options.AddPolicy("AllowAllCors", policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod();
});
```

**Problema**: `AllowAnyOrigin()` permite que **cualquier sitio web en internet** haga peticiones a la API, facilitando ataques CSRF desde sitios maliciosos y extracción de datos.

**Solución**:
```csharp
policy.WithOrigins("https://tudominio.com", "http://localhost:5000")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

---

### ✅ ~~1.4 Webhook de WhatsApp Sin Autenticación~~ — SOLUCIONADO

> **Resuelto**: Se agregó la propiedad `AppSecret` a `WhatsAppSettings` y se implementó la validación estricta de firma criptográfica **HMAC SHA-256 (`X-Hub-Signature-256`)** en `WhatsAppWebhookController.cs` con comparación segura en tiempo constante (`CryptographicOperations.FixedTimeEquals`), impidiendo la inyección no autorizada de mensajes a través del webhook.

**Archivo**: [WhatsAppWebhookController.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/WhatsAppWebhookController.cs)

**Problema**: El endpoint `POST /api/whatsapp/webhook` no tiene **ningún atributo de autorización** ni validación de firma (X-Hub-Signature-256 de Meta). Cualquier persona puede enviar mensajes falsos a la aplicación.

**Solución**:
```csharp
// Validar la firma X-Hub-Signature-256 del header de Meta
[HttpPost]
public async Task<IActionResult> ReceiveMessage(
    [FromHeader(Name = "X-Hub-Signature-256")] string? signature,
    [FromBody] JsonElement payload)
{
    if (!ValidateMetaSignature(signature, payload))
        return Unauthorized();
    // ... procesar
}
```

---

### ✅ ~~1.5 CSRF Bypass en GenerateJwtToken~~ — SOLUCIONADO

> **Resuelto**: Se eliminó el atributo `[IgnoreAntiforgeryToken]` y se reemplazó por `[ValidateAntiForgeryToken]` en `AccountController.cs`. Además, se configuró el servicio Antiforgery en `Program.cs` para admitir la cabecera `RequestVerificationToken` y se actualizó `DeveloperPanel.cshtml` enviando `@Html.AntiForgeryToken()` en la cabecera del request AJAX.

**Archivo**: [AccountController.cs L177-L179](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AccountController.cs#L177-L179)

```csharp
[Authorize(Roles = "Developer,Admin")]
[HttpPost]
[IgnoreAntiforgeryToken]  // ⚠️ Bypass de protección CSRF
public async Task<IActionResult> GenerateJwtToken(...)
```

**Problema**: `[IgnoreAntiforgeryToken]` desactiva la protección contra CSRF. Un sitio malicioso podría forzar al usuario a generar un token JWT sin su consentimiento.

**Solución**: Si este endpoint es consumido por JavaScript (AJAX), usar un enfoque basado en headers personalizados o tokens anti-CSRF en AJAX.

---

### 🟠 1.6 Sin Rate Limiting en Endpoints Sensibles

**Problema**: No existe rate limiting en:
- Login (`POST /Account/Login`)
- Registro (`POST /Account/Register`)
- API de autenticación JWT
- Webhook de WhatsApp
- Cálculo de hipoteca

**Riesgo**: Ataques de fuerza bruta en credenciales, abuso de recursos del servidor, DDoS a la API.

**Solución**: Agregar `Microsoft.AspNetCore.RateLimiting` en .NET 8+:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});
```

---

### 🟠 1.7 Sin Sanitización de Contenido de Chat (XSS)

**Archivo**: [Chat.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Domain/Entities/Chat.cs#L13) / [ChatService.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/ChatService.cs#L49-L83)

**Problema**: El contenido del mensaje (`MessageContent`) se almacena y muestra sin sanitización. Un usuario podría inyectar `<script>` tags que se ejecutarían en el navegador de otros usuarios.

**Solución**:
```csharp
// Antes de guardar
chat.MessageContent = System.Net.WebUtility.HtmlEncode(vm.MessageContent);
// O usar una librería como HtmlSanitizer de NuGet
```

---

### 🟠 1.8 Contraseña Mínima Débil (6 caracteres)

**Archivo**: [Persistence ServiceRegistration.cs L48](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/ServiceRegistration.cs#L48)

```csharp
options.Password.RequiredLength = 6;
```

**Solución**: Subir a mínimo 8 caracteres, idealmente 12. El estándar OWASP recomienda mínimo 8.

---

### 🟡 1.9 Credenciales de Prueba Documentadas en README

**Archivo**: [README.md L68-L73](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/README.md#L68-L73)

**Problema**: Contraseñas de prueba (`Admin123!`, `Agent123!`, `Client123!`) documentadas públicamente. Si el seed se ejecuta en producción, estas cuentas son vulnerables.

**Solución**: Deshabilitar seeds de usuarios de prueba en ambiente de producción. Usar `IHostEnvironment.IsDevelopment()` como guard.

---

### 🟡 1.10 Swagger Habilitado en Producción

**Archivo**: [WebApi Program.cs L146-L151](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs#L146-L151)

```csharp
// Enable Swagger UI in development and production
app.UseSwagger();
app.UseSwaggerUI(...)
```

**Problema**: Swagger expone la documentación completa de todos los endpoints, facilitando el reconocimiento por parte de atacantes.

**Solución**: Limitar a desarrollo:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(...);
}
```

---

## 🏗️ 2. Defectos Arquitectónicos

### 🟠 2.1 Violación de Onion Architecture: Identity en Application Layer

**Archivo**: [AgentService.cs L6](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/AgentService.cs#L6)

```csharp
using Microsoft.AspNetCore.Identity;
// ...
private readonly UserManager<IdentityUser> _userManager;
```

**Problema**: La capa `Core.Application` tiene dependencia directa de `Microsoft.AspNetCore.Identity`, que es una librería de infraestructura. Esto **viola el principio fundamental de la Arquitectura Onion**: el core no debe depender de infraestructura.

**Solución**: Crear una interfaz `IUserRepository` o `IUserService` en Application que abstraiga las operaciones de Identity. Implementarla en la capa de Persistence.

---

### 🟠 2.2 SaveChanges por Cada Operación Individual (N+1 Problem)

**Archivo**: [GenericRepository.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/GenericRepository.cs)

```csharp
public virtual async Task<T> AddAsync(T entity)
{
    await _dbContext.Set<T>().AddAsync(entity);
    await _dbContext.SaveChangesAsync();  // ⚠️ Save en cada Add
    return entity;
}
```

**Impacto crítico en** [DeleteAgentCascadeAsync](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/AgentService.cs#L194-L257): Si un agente tiene 10 propiedades con 5 imágenes, 3 ofertas, 2 chats y 4 favoritos cada una, se ejecutan **~200 SaveChangesAsync individuales** en lugar de 1.

**Solución**: Implementar Unit of Work pattern:
```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

---

### 🟠 2.3 Falta de Transacciones en Operaciones Atómicas

**Archivo**: [OfferService.AcceptOffer](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/OfferService.cs#L78-L106)

**Problema**: La "Regla de Negocio Atómica" (aceptar oferta → cambiar propiedad a Vendida → rechazar otras ofertas) ejecuta 3+ operaciones independientes sin transacción. Si el servidor cae entre el paso 1 y 2, la oferta queda aceptada pero la propiedad sigue "Disponible".

**Solución**:
```csharp
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try
{
    // 1. Aceptar oferta
    // 2. Cambiar estado propiedad
    // 3. Rechazar ofertas pendientes
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

### 🟡 2.4 Archivos Class1.cs Placeholder Residuales

**Archivos**:
- [Domain/Class1.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Domain/Class1.cs)
- [Application/Class1.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Class1.cs)
- [Persistence/Class1.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Class1.cs)
- [Shared/Class1.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Shared/Class1.cs)

**Solución**: Eliminar los 4 archivos placeholder.

---

## 🐛 3. Bugs Funcionales

### ✅ ~~3.1 Carta de Pre-Aprobación Bancaria No Se Almacena~~ — SOLUCIONADO

> **Resuelto**: Se agregó la propiedad `PreApprovalLetterUrl` a la entidad `Offer`, a `SaveOfferViewModel` y `OfferViewModel`. En `OffersController.cs` se asignó `vm.PreApprovalLetterUrl = letterUrl` tras la carga del archivo, persistiendo la ruta en la base de datos. Además, en `Agent/Offers.cshtml` se agregó el botón **"Ver Carta"** para que los agentes puedan revisar y descargar los documentos adjuntos de pre-aprobación bancaria.

**Archivo**: [OffersController.cs L77-L82](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/OffersController.cs#L77-L82)

```csharp
if (vm.PreApprovalLetter != null && vm.PreApprovalLetter.Length > 0)
{
    using var stream = vm.PreApprovalLetter.OpenReadStream();
    var letterUrl = await _fileStorageService.UploadFileAsync(stream, vm.PreApprovalLetter.FileName, "preapprovals");
    // La URL de la carta queda almacenada para revisión por el agente
    // ⚠️ PERO NO SE GUARDA EN NINGÚN LADO - letterUrl se descarta
}
```

**Problema**: El archivo se sube correctamente al disco, pero la URL resultante (`letterUrl`) **nunca se guarda** en la entidad Offer ni en ninguna tabla. El agente no puede acceder al documento.

**Solución**: Agregar campo `PreApprovalLetterUrl` a la entidad `Offer` y guardarlo:
```csharp
offer.PreApprovalLetterUrl = letterUrl;
```

---

### ✅ ~~3.2 Chat de Soporte con PropertyId Inexistente en BD~~ — SOLUCIONADO

> **Resuelto**: Se convirtió la propiedad `PropertyId` a `int?` (nullable) en `Chat.cs`, `ChatViewModel.cs` y `SaveChatViewModel.cs`. Se configuró la clave foránea como opcional (`.IsRequired(false)`) en `ApplicationDbContext.cs`. En `ChatRepository.cs` y `ChatService.cs` se mapean los canales de soporte (PropertyId <= 0) a `PropertyId = null` en la base de datos, garantizando cumplimiento del 100% de la integridad referencial SQL Server sin violaciones de Clave Foránea (`FK_Chats_Properties_PropertyId`).

**Archivo**: [ChatsController.cs L58 y L78](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/ChatsController.cs#L58)

```csharp
// Soporte Técnico: PropertyId = 0
return RedirectToAction(nameof(Thread), new { propertyId = 0, ... });

// Soporte Admin: PropertyId = -1
return RedirectToAction(nameof(Thread), new { propertyId = -1, ... });
```

**Problema**: La tabla `Chats` tiene una FK `PropertyId` hacia `Properties` con cascade delete. Los IDs `0` y `-1` **no existen** en la tabla Properties. Esto funciona solo porque EF Core no valida la FK a nivel de aplicación en algunos escenarios, pero:
- Integridad referencial rota en la base de datos
- Queries con Include de Property fallarán para estos chats

**Solución**: Crear una propiedad de soporte especial en la tabla Properties o hacer `PropertyId` nullable y usar `null` para chats de soporte.

---

### 🟠 3.3 Nombres de Agentes Incorrectos (No Lee Claims)

**Archivo**: [AgentService.cs L68-L69](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/AgentService.cs#L68-L69)

```csharp
agentVms.Add(new AgentViewModel
{
    FirstName = user.UserName ?? string.Empty,  // ⚠️ Usa UserName como nombre
    LastName = string.Empty,                     // ⚠️ Siempre vacío
    ...
});
```

**Problema**: El servicio no lee los Claims `"FirstName"` y `"LastName"` que sí se guardan durante el registro (ver [AccountService.cs L308-L309](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs#L308-L309)). El resultado es que todos los agentes muestran su username como nombre y apellido vacío.

**Solución**:
```csharp
var claims = await _userManager.GetClaimsAsync(user);
var firstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value ?? user.UserName;
var lastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? string.Empty;
```

---

### 🟠 3.4 Ruta de Confirmación de Email Apunta al Endpoint Incorrecto

**Archivo**: [AccountService.cs L160-L161](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs#L160-L161)

```csharp
var route = "api/v1/account/confirm-email";
var verificationUri = $"{origin}/{route}?userId={user.Id}&token={encodedToken}";
```

**Problema**: La ruta `api/v1/account/confirm-email` apunta a la **Web API**, pero el `ConfirmEmail` action está en la **WebApp** en [AccountController.cs L197](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AccountController.cs#L197).

**Solución**: Cambiar la ruta a:
```csharp
var route = "Account/ConfirmEmail";
```

---

### 🟠 3.5 Código Único de Propiedad Puede Colisionar

**Archivo**: [PropertyService.cs L236-L239](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs#L236-L239)

```csharp
private static string GenerateUniqueCode()
{
    return Guid.NewGuid().ToString("N")[..6].ToUpper();
}
```

**Problema**: Solo usa 6 caracteres de un GUID (36^6 = ~2.17 mil millones combinaciones), pero NO verifica si el código ya existe en la base de datos. Con muchas propiedades, las colisiones son posibles. Además, la tabla tiene un índice UNIQUE en el campo `Code`, por lo que una colisión causaría un error no manejado.

**Solución**:
```csharp
private async Task<string> GenerateUniqueCodeAsync()
{
    string code;
    do
    {
        code = Guid.NewGuid().ToString("N")[..6].ToUpper();
    } while (await _propertyRepository.ExistsByCodeAsync(code));
    return code;
}
```

---

### 🟡 3.6 Auditoría Siempre Registra "System" como Usuario

**Archivo**: [ApplicationDbContext.cs L296-L300](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Contexts/ApplicationDbContext.cs#L294-L301)

```csharp
case EntityState.Added:
    entry.Entity.Created = DateTime.UtcNow;
    entry.Entity.CreatedBy = "System"; // ⚠️ Siempre "System"
    break;
case EntityState.Modified:
    entry.Entity.LastModified = DateTime.UtcNow;
    entry.Entity.LastModifiedBy = "System"; // ⚠️ Siempre "System"
    break;
```

**Solución**: Inyectar `IHttpContextAccessor` en el DbContext y leer el usuario actual:
```csharp
var userId = _httpContextAccessor.HttpContext?.User
    ?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
```

---

### 🟡 3.7 DeleteProperty del Agente No Verifica Propiedad del Agente

**Archivo**: [AgentController.cs L140-L147](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AgentController.cs#L140-L147)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteProperty(int id)
{
    await _propertyService.Delete(id); // ⚠️ No verifica que el agente sea dueño
    ...
}
```

**Problema**: Un agente podría manipular el formulario para eliminar propiedades de **otro** agente. La acción `EditProperty` sí tiene esta validación (L110), pero `DeleteProperty` no.

**Solución**: Agregar verificación de propiedad:
```csharp
var property = await _propertyService.GetByIdViewModel(id);
if (property == null || property.AgentId != _userManager.GetUserId(User))
    return Forbid();
```

---

## ⚡ 4. Defectos de Rendimiento

### 🔴 4.1 Filtrado de Propiedades Carga Todo a Memoria

**Archivo**: [PropertyService.cs L170-L207](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs#L170-L207)

```csharp
public async Task<List<PropertyViewModel>> GetAllWithFilters(PropertyFilterViewModel filters)
{
    var allProperties = await _propertyRepository.GetAllAsync(); // ⚠️ CARGA TODAS
    var query = allProperties.AsEnumerable(); // ⚠️ Filtra en C#, no en SQL
    ...
}
```

**Problema**: Con 10,000 propiedades, **todas** se cargan en memoria antes de filtrar. Esto causa:
- Alto consumo de RAM
- Tiempo de respuesta lento
- Carga innecesaria al servidor de BD

**Mismo problema en**:
- [GetByAgentId L209-L213](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs#L209-L213)
- [GetByCode L216-L224](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs#L216-L224)

**Solución**: Usar `IQueryable<T>` y construir la query dinámicamente:
```csharp
public async Task<List<PropertyViewModel>> GetAllWithFilters(PropertyFilterViewModel filters)
{
    IQueryable<Property> query = _dbContext.Properties.AsQueryable();
    
    if (!string.IsNullOrWhiteSpace(filters.Code))
        query = query.Where(p => p.Code == filters.Code);
    
    if (filters.PropertyTypeId.HasValue)
        query = query.Where(p => p.PropertyTypeId == filters.PropertyTypeId);
    
    // ... etc. Todo se traduce a SQL
    return _mapper.Map<List<PropertyViewModel>>(await query.ToListAsync());
}
```

---

### 🟠 4.2 DeleteAgentCascade Ejecuta Cientos de Queries Individuales

**Archivo**: [AgentService.cs L194-L257](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/AgentService.cs#L194-L257)

**Problema**: Para cada propiedad del agente, se ejecutan queries individuales para imágenes, favoritos, ofertas y chats. Luego cada eliminación es un SaveChanges individual.

**Ejemplo**: Agente con 5 propiedades × (4 imágenes + 3 favoritos + 2 ofertas + 5 chats) = **70+ queries y 70+ SaveChanges**.

**Solución**: Usar `RemoveRange` y un solo `SaveChangesAsync`:
```csharp
var propertyIds = agentProperties.Select(p => p.Id).ToList();
var images = await _dbContext.PropertyImages.Where(i => propertyIds.Contains(i.PropertyId)).ToListAsync();
_dbContext.PropertyImages.RemoveRange(images);
// ... similar para favoritos, ofertas, chats
await _dbContext.SaveChangesAsync(); // Un solo save
```

---

### 🟠 4.3 Sin Paginación en Ningún Listado

**Problema**: Todos los `GetAll()` retornan **TODOS** los registros:
- Propiedades, Ofertas, Chats, Favoritos, Agentes

Con crecimiento de datos, las vistas se volverán lentas e inutilizables.

---

### 🟡 4.4 Agente.Offers Carga Todas las Ofertas del Sistema

**Archivo**: [AgentController.Offers L159](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AgentController.cs#L159)

```csharp
var allOffers = await _offerService.GetAllViewModel(); // ⚠️ TODAS las ofertas
var agentOffers = allOffers.FindAll(o => agentPropertyIds.Contains(o.PropertyId));
```

**Solución**: Crear método `GetByPropertyIdsAsync(IEnumerable<int> propertyIds)` en el repositorio.

---

## ⚙️ 5. Problemas de Configuración

### 🟠 5.1 AllowedHosts Acepta Todo

**Archivos**: Ambos `appsettings.json`

```json
"AllowedHosts": "*"
```

**Problema**: En producción, esto permite peticiones con cualquier valor de Host header, facilitando ataques de Host Header Injection.

**Solución**: En producción, especificar los dominios exactos.

---

### 🟡 5.2 HTTPS Redirection Sin HSTS Preload

**Archivo**: [WebApp Program.cs L79-L80](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs#L79-L80)

**Problema**: HSTS está configurado con valor por defecto (30 días) solo en no-development. No incluye `includeSubDomains` ni `preload`.

**Solución**:
```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});
```

---

### 🟡 5.3 WebApi No Usa HTTPS Redirection

**Archivo**: [WebApi Program.cs](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs)

**Problema**: La Web API no llama `app.UseHttpsRedirection()` ni `app.UseHsts()`. Todo el tráfico API puede viajar sin cifrar.

---

## 🧹 6. Deuda Técnica

### 🟡 6.1 Strings Mágicos para Estados de Propiedad

**Múltiples archivos**: El estado de propiedad usa strings como `"Disponible"`, `"Reservada"`, `"Vendida"` dispersos en todo el código:
- [Property.cs L15](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Domain/Entities/Property.cs#L15)
- [PropertyService.cs L67](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs#L67)
- [OfferService.cs L59, L95](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/OfferService.cs#L59)
- [AdminController.cs L42-L44](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Controllers/AdminController.cs#L42-L44)

**Solución**: Crear un enum `PropertyStatus` similar a `OfferStatus`.

---

### 🟡 6.2 Duplicación de Lógica de Verificación de Estado Activo

La lógica para verificar si un usuario está activo se repite en al menos **5 archivos**:
```csharp
var isActive = !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;
```

**Solución**: Crear un método de extensión:
```csharp
public static bool IsActiveUser(this IdentityUser user)
    => !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;
```

---

### 🟡 6.3 Manejo de Excepciones con `throw new Exception()`

**Múltiples archivos**: Los servicios usan `throw new Exception("mensaje")` para errores de negocio en lugar de excepciones tipadas.

**Solución**: Crear excepciones de dominio:
```csharp
public class BusinessRuleException : Exception { ... }
public class EntityNotFoundException : Exception { ... }
```

---

### 🟢 6.4 Seeds de Datos Ejecutan en Producción

**Archivos**: [WebApp Program.cs L50-L73](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApp/Program.cs#L50-L73) y [WebApi Program.cs L121-L143](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs#L121-L143)

**Solución**:
```csharp
if (app.Environment.IsDevelopment())
{
    // Seeds solo en desarrollo
    await DefaultRealEstateData.SeedAsync(dbContext, userManager);
}
```

---

### 🟢 6.5 SizeInMeters Usa `decimal` Pero Tiene Valores con .50

**Archivo**: [Property.cs L12](file:///c:/Users/DELL/Desktop/New%20folder/RealEstateApp/src/Core/RealEstateApp.Core.Domain/Entities/Property.cs#L12)

**Problema menor**: `SizeInMeters` no tiene configuración de precisión en el DbContext (no se define `HasColumnType("decimal(10,2)")` como sí se hace para `Price` y `MontoSeparacion`).

---

## 📊 Resumen de Hallazgos

| Severidad | Categoría | Cantidad |
|-----------|-----------|----------|
| 🔴 Crítico | Seguridad | 5 |
| 🔴 Crítico | Bugs Funcionales | 2 |
| 🔴 Crítico | Rendimiento | 1 |
| 🟠 Alto | Seguridad | 3 |
| 🟠 Alto | Arquitectura | 3 |
| 🟠 Alto | Bugs Funcionales | 3 |
| 🟠 Alto | Rendimiento | 2 |
| 🟠 Alto | Configuración | 1 |
| 🟡 Medio | Seguridad | 2 |
| 🟡 Medio | Bugs Funcionales | 2 |
| 🟡 Medio | Rendimiento | 1 |
| 🟡 Medio | Deuda Técnica | 4 |
| 🟢 Bajo | Deuda Técnica | 2 |
| **Total** | | **31** |

> [!IMPORTANT]
> Se recomienda abordar primero los **8 hallazgos críticos** (🔴) antes de continuar con el desarrollo de nuevas funcionalidades.
