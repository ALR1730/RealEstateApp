# Contrato ALPHA — Fase 0: Contención P0 (Seguridad Crítica, Crashes, XSS y Reglas)

> **Fase:** 0 de 3 (`fase-0-contencion-p0`)  
> **Prioridad:** Crítica / Bloqueante (cierre de vulnerabilidades explotables y fallos de runtime hoy)  
> **Migraciones de BD:** **Ninguna** (`0` cambios de esquema)  
> **Roles líderes:** Security Auditor (`DELTA`) + Principal Engineer (`GAMMA`) + QA & TDD (`alr-qa-tdd`)

---

## 1. Alcance y Archivos Afectados por Capa

| Capa | Archivo | Responsabilidad / Cambio |
| :--- | :--- | :--- |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/Interfaces/Services/IOfferService.cs` | Añadir `string agentUserId` a `AcceptOffer` y `RejectOffer` para validación de propiedad. |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/Services/OfferService.cs` | Validar que `property.AgentId == agentUserId` (o rechazar con `ValidationException` / autorización) en `AcceptOffer`, `RejectOffer` y `CounterOffer`. |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/Interfaces/Services/IFileValidationService.cs` *(Nuevo)* | Contrato único de Triple Barrera (`FileUploadPolicy`: tamaño máx., extensiones permitidas, MIME y magic bytes). |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/Services/FileValidationService.cs` *(Nuevo)* | Implementación centralizada reutilizada por imágenes, documentos y verificaciones KYC. |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/Services/PropertyDocumentService.cs` | Aplicar `IFileValidationService` con política de documentos (`pdf`, `jpg`, `jpeg`, `png`, máx. `10 MB` + magic bytes), bloqueando `.html`/`.svg`/`.js`/ejecutables. |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/Services/AgentVerificationService.cs` | Aplicar `IFileValidationService` con política de imágenes KYC (`jpg`, `jpeg`, `png`, `webp`, máx. `5 MB` + magic bytes) y validar campos requeridos. |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/Services/PropertyImageService.cs` | Delegar validación de magic bytes/extensión/tamaño a `IFileValidationService` (sin duplicar lógica). |
| **Core.Application** | `src/Core/RealEstateApp.Core.Application/ServiceRegistration.cs` | Registrar `IFileValidationService`. |
| **Infrastructure.Shared** | `src/Infrastructure/RealEstateApp.Infrastructure.Shared/Services/FinancingService.cs` | Validar parámetros de entrada antes de operar (`propertyPrice <= 0`, `downPayment < 0 \|\| downPayment >= propertyPrice`, `annualRate < 0 \|\| annualRate > 100`, `termInYears < 1 \|\| termInYears > 40`) evitando `DivideByZeroException`, `OverflowException` y DoS por memoria. |
| **Infrastructure.Shared** | `src/Infrastructure/RealEstateApp.Infrastructure.Shared/Services/FileStorageService.cs` | Normalizar ruta con `Path.GetFullPath` y exigir prefijo `_basePath` (separador incluido) en `DeleteFileAsync` y validación de `containerName` en `UploadFileAsync` contra path traversal. |
| **Infrastructure.Shared** | `src/Infrastructure/RealEstateApp.Infrastructure.Shared/Services/EmailService.cs` | Evitar exposición de tokens/enlaces sensibles en logs de consola en entornos no-Development (enmascarar cuerpo cuando no haya SMTP). |
| **Infrastructure.Persistence** | `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs` | Unificar mensajes genéricos en `ForgotPasswordAsync` y `ResetPasswordAsync` para impedir enumeración de correos registrados. |
| **Presentation.WebApi** | `src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/OffersController.cs` | Extraer `agentId` desde `User` y pasarlo a `AcceptOffer` y `RejectOffer`. |
| **Presentation.WebApi** | `src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AccountController.cs`, `AiSearchController.cs`, `SimulatorController.cs` | Aplicar `[EnableRateLimiting]` en endpoints sensibles/costosos (`register-*`, `forgot-password`, `reset-password`, `ai-search`, `simulator/calculate`). |
| **Presentation.WebApi** | `src/Presentation/RealEstateApp.Presentation.WebApi/Hubs/NotificationHub.cs` | Eliminar el método invocable por clientes `SendNotificationToUser` (las notificaciones deben emitirse exclusivamente desde el servidor vía `IHubContext<NotificationHub>`). |
| **Presentation.WebApi** | `src/Presentation/RealEstateApp.Presentation.WebApi/Hubs/ChatHub.cs` | Validar existencia de propiedad, autorización del participante, longitud máxima del mensaje (`<= 2000` caracteres) y timestamp UTC (`DateTime.UtcNow`). |
| **Presentation.WebApi** | `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs` | Eliminar wildcard `uri.Host.EndsWith(".onrender.com")` en CORS (solo orígenes explícitos de `AllowedOrigins` y `FRONTEND_URL`); fijar `RequireHttpsMetadata = !builder.Environment.IsDevelopment()`. |
| **Presentation.WebApi** | `src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.json` y `appsettings.Development.json` | Retirar `JWTSettings:Key` y `UseInMemoryDatabase: true` de `appsettings.json`; mover configuración local de desarrollo a `appsettings.Development.json` y `dotnet user-secrets`. |
| **Presentation.ClientApp** | `src/Presentation/RealEstateApp.ClientApp/src/utils/urlValidator.ts` *(Nuevo)* | Función pura `isSafeEmbedUrl(url: string): boolean` (solo `https://` y hosts de confianza: `youtube.com`, `youtu.be`, `vimeo.com`, `matterport.com`, `my.matterport.com`, `kuula.co`, `roundme.com`). |
| **Presentation.ClientApp** | `src/Presentation/RealEstateApp.ClientApp/src/components/properties/VideoPlayer.tsx` y `VirtualTourEmbed.tsx` | Validar URL con `isSafeEmbedUrl`, mostrar fallback seguro si es inválida, y añadir `sandbox="allow-scripts allow-same-origin allow-presentation"` y `referrerPolicy="no-referrer"` al `<iframe>`. |
| **DevOps / Config** | `render.yaml` | Fijar `SeedInitialData: "false"`, `EnableSwaggerInProduction: "false"`, y `JWTSettings__DurationInMinutes: "60"`. |
| **Workspace Rules** | `.agents/rules/architecture_standards.md` y `.agents/rules/soul_robert_c_martin.md` | Agregar frontmatter `trigger: always_on` para carga automática garantizada. |

---

## 2. Firmas de Contrato para `alr-qa-tdd` (Fase Red)

```csharp
// IOfferService (Core.Application/Interfaces/Services/IOfferService.cs)
Task AcceptOffer(int offerId, string agentUserId);
Task RejectOffer(int offerId, string agentUserId);
Task CounterOffer(int offerId, decimal counterAmount, string? counterMessage, string agentUserId);

// IFileValidationService (Core.Application/Interfaces/Services/IFileValidationService.cs)
public enum FileUploadPolicy
{
    PropertyImage,      // .jpg, .jpeg, .png, .webp | <= 10 MB | magic bytes
    VerificationImage,  // .jpg, .jpeg, .png, .webp | <= 5 MB  | magic bytes
    LegalDocument       // .pdf, .jpg, .jpeg, .png   | <= 10 MB | magic bytes (%PDF-, JPEG, PNG)
}

public interface IFileValidationService
{
    Task ValidateAsync(Stream fileStream, string fileName, string contentType, FileUploadPolicy policy);
}
```

```typescript
// src/Presentation/RealEstateApp.ClientApp/src/utils/urlValidator.ts
export function isSafeEmbedUrl(url: string): boolean;
```

---

## 3. Criterios de Aceptación (`GIVEN / WHEN / THEN`)

1. **BOLA en Ofertas (`AcceptOffer` / `RejectOffer` / `CounterOffer`):**
   - `GIVEN` una oferta sobre una propiedad perteneciente al `Agent-A`
   - `WHEN` el `Agent-B` autenticado intenta ejecutar `AcceptOffer`, `RejectOffer` o `CounterOffer` con su propio `agentUserId`
   - `THEN` el servicio lanza `ValidationException` sin modificar el estado de la oferta ni de la propiedad.
2. **Spoofing en `NotificationHub`:**
   - `GIVEN` un cliente conectado al Hub `/hubs/notifications`
   - `WHEN` intenta invocar `SendNotificationToUser` desde el cliente SignalR
   - `THEN` el método no existe en la superficie pública del Hub (el Hub rechaza la invocación).
3. **Integridad en `ChatHub`:**
   - `GIVEN` un usuario autenticado que no es el agente propietario del inmueble ni participante válido del hilo, o envía un mensaje vacío / `> 2000` caracteres
   - `WHEN` invoca `JoinThread` o `SendMessage`
   - `THEN` el Hub rechaza el envío sin persistir ni retransmitir el mensaje, y los mensajes válidos registran marca temporal UTC.
4. **Triple Barrera en `PropertyDocumentService`:**
   - `GIVEN` un agente dueño de la propiedad intentando subir `payload.html`, `vector.svg` o un `.pdf` cuyo binario real no inicia con `%PDF-` o excede `10 MB`
   - `WHEN` llama `UploadAsync`
   - `THEN` `IFileValidationService` lanza `ValidationException` y `IFileStorageService.UploadFileAsync` nunca es invocado.
5. **Triple Barrera en `AgentVerificationService`:**
   - `GIVEN` un agente enviando un archivo ejecutables/HTML renombrado a `.jpg` o de `> 5 MB` en `FrontImageFile`/`BackImageFile`
   - `WHEN` llama `SubmitVerificationAsync`
   - `THEN` se rechaza la carga antes de escribir en disco/storage y no se registra la verificación.
6. **Protección contra Path Traversal en `FileStorageService`:**
   - `GIVEN` una ruta maliciosa como `"/../../appsettings.json"` o `"..\\..\\secret.txt"` pasada a `DeleteFileAsync`
   - `WHEN` se invoca `DeleteFileAsync`
   - `THEN` lanza `ValidationException` o `UnauthorizedAccessException` sin acceder fuera de `_basePath`.
7. **Robustez matemática y anti-DoS en `FinancingService`:**
   - `GIVEN` entradas inválidas (`propertyPrice <= 0`, `downPayment < 0`, `downPayment >= propertyPrice`, `annualRate < 0`, `annualRate > 100`, `termInYears <= 0`, `termInYears > 40`)
   - `WHEN` se invoca directamente `CalculateMortgage` o `GenerateAmortizationSchedule`
   - `THEN` lanza `ValidationException` inmediatamente sin producir `DivideByZeroException`, `OverflowException` ni bucles de memoria.
8. **Anti-XSS en `VideoPlayer.tsx` y `VirtualTourEmbed.tsx`:**
   - `GIVEN` una URL con esquema `javascript:alert(1)`, `data:text/html,...`, `http://` inseguro o dominio no autorizado `https://evil.example.com/embed`
   - `WHEN` se renderiza `<VideoPlayer url={...} />` o `<VirtualTourEmbed url={...} />`
   - `THEN` **no** se renderiza ningún `<iframe>` con esa URL y se muestra mensaje seguro de enlace no válido; cuando el host es válido (`https://www.youtube.com/...`, `https://my.matterport.com/...`), el `<iframe>` incluye `sandbox` y `referrerPolicy="no-referrer"`.
9. **Hardening de CORS, Secretos, Enumeración y `render.yaml`:**
   - `GIVEN` `appsettings.json`, `Program.cs` y `render.yaml`
   - `WHEN` se inspeccionan en producción
   - `THEN` `appsettings.json` no contiene secreto JWT ni `UseInMemoryDatabase: true`; una petición CORS desde `https://attacker.onrender.com` no recibe `Access-Control-Allow-Origin`; `ForgotPasswordAsync` responde idéntico para correos existentes e inexistentes; `render.yaml` tiene `SeedInitialData: "false"` y `EnableSwaggerInProduction: "false"`.
10. **Reglas del Workspace cargadas:**
    - `GIVEN` `.agents/rules/architecture_standards.md` y `.agents/rules/soul_robert_c_martin.md`
    - `WHEN` inicia una sesión en el repositorio
    - `THEN` ambas poseen frontmatter `trigger: always_on`.

---

## 4. Riesgos y Dependencias de Fase

- El cambio de firma en `IOfferService.AcceptOffer` y `IOfferService.RejectOffer` impactará a `OffersController`, `OfferServiceTests` y `OfferServiceStateTests` (deben actualizarse de forma coordinada en la fase Red/Green).
- Al retirar `JWTSettings:Key` de `appsettings.json`, el entorno local necesitará `dotnet user-secrets set "JWTSettings:Key" "..."` o variable de entorno `JWTSettings__Key` para levantar `WebApi` en desarrollo.
