# Reporte Técnico de Ejecución — Fase 0 (Contención P0)

> **Proyecto:** `RealEstateApp`  
> **Contrato ejecutado:** [plan.md](./plan.md)  
> **Flujo aplicado:** `alr-alpha` → `alr-qa-tdd` (Fase Red aislada) → Implementación Green/Refactor → Auditoría `alr-delta` & `alr-zeta`  
> **Migraciones de BD:** `0` (sin cambios de esquema)

---

## 1. Resumen de Alcance y Métricas

| Métrica | Valor observado |
| :--- | :--- |
| **Archivos modificados (tracked)** | `30` archivos (`+416` inserciones / `-156` eliminaciones) |
| **Archivos nuevos de producción** | `3` ([IFileValidationService.cs](../../../src/Core/RealEstateApp.Core.Application/Interfaces/Services/IFileValidationService.cs), [FileValidationService.cs](../../../src/Core/RealEstateApp.Core.Application/Services/FileValidationService.cs), [urlValidator.ts](../../../src/Presentation/RealEstateApp.ClientApp/src/utils/urlValidator.ts)) |
| **Suites de prueba nuevas (`alr-qa-tdd`)** | `7` archivos (`2` Frontend Vitest + `5` Backend xUnit) |
| **Suites existentes actualizadas** | `4` archivos Backend (`0` tests borrados — Guardarraíl #3 cumplido) |
| **Frontend Vitest (antes → después)** | `11` archivos / `88` tests → **`13` archivos / `123` tests pasando (`+35` tests nuevos)** |

---

## 2. Matriz de Brechas P0 Cerradas (Antes vs. Después)

| # | Riesgo / Defecto P0 | Estado Anterior | Solución Implementada | Archivos |
| :-: | :--- | :--- | :--- | :--- |
| **0.1** | **BOLA / IDOR en Ofertas** | Cualquier agente autenticado podía aceptar, rechazar o contraofertar ofertas de propiedades de otro agente adivinando `{id:int}`. | `AcceptOffer(offerId, agentUserId)`, `RejectOffer(offerId, agentUserId)` y `CounterOffer` validan propiedad del inmueble vía `ValidateAgentOwnsOfferPropertyAsync`; `OffersController` inyecta el `ClaimTypes.NameIdentifier` del JWT. | [IOfferService.cs](../../../src/Core/RealEstateApp.Core.Application/Interfaces/Services/IOfferService.cs), [OfferService.cs](../../../src/Core/RealEstateApp.Core.Application/Services/OfferService.cs), [OffersController.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/OffersController.cs) |
| **0.2a** | **Spoofing / Phishing en `NotificationHub`** | Cualquier usuario autenticado podía invocar `SendNotificationToUser` desde el cliente SignalR y enviar alertas falsas con `redirectUrl` arbitrarios a cualquier usuario (incluyendo Admins). | Se eliminó por completo el método público `SendNotificationToUser` de la superficie del Hub (solo emisión server-side mediante `IHubContext<NotificationHub>`). | [NotificationHub.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Hubs/NotificationHub.cs) |
| **0.2b** | **Falta de control de acceso y límites en `ChatHub` / `ChatService`** | `JoinThread` y `SendMessage` no validaban pertenencia a la propiedad, aceptaban mensajes de tamaño ilimitado y emitían `DateTime.Now` local. | Se agregó `CanUserAccessThreadAsync` en `IChatService`/`ChatService`; validación obligatoria en `JoinThread`, `SendMessage` y `SendTyping`; límite `MaxMessageLength = 2000`; `WebUtility.HtmlEncode` en payload y marcas temporales `DateTime.UtcNow`. | [IChatService.cs](../../../src/Core/RealEstateApp.Core.Application/Interfaces/Services/IChatService.cs), [ChatService.cs](../../../src/Core/RealEstateApp.Core.Application/Services/ChatService.cs), [ChatHub.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Hubs/ChatHub.cs) |
| **0.3a** | **Uploads sin Triple Barrera (Stored XSS / Malware)** | Solo `PropertyImageService` validaba magic bytes; `PropertyDocumentService` permitía `.html`/`.svg`/`.js` y `AgentVerificationService` no validaba nada. | Se creó `IFileValidationService` / `FileValidationService` (registrado en DI) con 3 políticas (`PropertyImage` `<= 10 MB`, `VerificationImage` `<= 5 MB`, `LegalDocument` `<= 10 MB`), whitelist de extensiones, correspondencia estricta extensión↔MIME y lectura de magic bytes reales (JPEG, PNG, WebP, `%PDF-`). Integrado en los 3 servicios. | [IFileValidationService.cs](../../../src/Core/RealEstateApp.Core.Application/Interfaces/Services/IFileValidationService.cs), [FileValidationService.cs](../../../src/Core/RealEstateApp.Core.Application/Services/FileValidationService.cs), [ServiceRegistration.cs](../../../src/Core/RealEstateApp.Core.Application/ServiceRegistration.cs), [PropertyDocumentService.cs](../../../src/Core/RealEstateApp.Core.Application/Services/PropertyDocumentService.cs), [AgentVerificationService.cs](../../../src/Core/RealEstateApp.Core.Application/Services/AgentVerificationService.cs), [PropertyImageService.cs](../../../src/Core/RealEstateApp.Core.Application/Services/PropertyImageService.cs) |
| **0.3b** | **Path Traversal en `FileStorageService`** | `DeleteFileAsync` combinaba rutas sin validar escapes (`..`) y `UploadFileAsync` no sanitizaba `containerName`. | Validación regex estricta `^[a-zA-Z0-9_-]+$` para `containerName` y verificación canónica con `Path.GetFullPath` asegurando prefijo dentro de `_basePath` antes de crear o borrar archivos. | [FileStorageService.cs](../../../src/Infrastructure/RealEstateApp.Infrastructure.Shared/Services/FileStorageService.cs) |
| **0.4** | **Crash / Overflow en `FinancingService` y endpoints públicos sin Rate Limit** | Llamar `FinancingService` con `termInYears <= 0` o valores extremos causaba `DivideByZeroException` / `OverflowException` / agotamiento de memoria; endpoints de registro, recuperación, IA y simulador sin rate limiting. | Guardas defensivas internas en `FinancingService` (`propertyPrice > 0`, `0 <= downPayment < propertyPrice`, `0 <= annualRate <= 100`, `1 <= termInYears <= 40`); nueva política `"PublicComputePolicy"` (`30 req/min`) y `[EnableRateLimiting]` en `AccountController`, `AiSearchController` y `SimulatorController`. | [FinancingService.cs](../../../src/Infrastructure/RealEstateApp.Infrastructure.Shared/Services/FinancingService.cs), [Program.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs), [AccountController.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AccountController.cs), [AiSearchController.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AiSearchController.cs), [SimulatorController.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/SimulatorController.cs) |
| **0.5** | **Stored XSS / Iframe arbitrario en Frontend** | `VideoPlayer.tsx` y `VirtualTourEmbed.tsx` inyectaban `<iframe src={url} />` sin validar esquema (`javascript:`, `data:`), sin whitelist y sin `sandbox`. | Utilidad `isSafeEmbedUrl(url)` (`https://` + whitelist de proveedores confiables, evaluada antes de `extractYouTubeId`); fallback seguro si la URL es rechazada; `sandbox="allow-scripts allow-same-origin allow-presentation"` y `referrerPolicy="no-referrer"` en todos los `<iframe>`. | [urlValidator.ts](../../../src/Presentation/RealEstateApp.ClientApp/src/utils/urlValidator.ts), [VideoPlayer.tsx](../../../src/Presentation/RealEstateApp.ClientApp/src/components/properties/VideoPlayer.tsx), [VirtualTourEmbed.tsx](../../../src/Presentation/RealEstateApp.ClientApp/src/components/properties/VirtualTourEmbed.tsx) |
| **0.6** | **Secretos en Git, CORS wildcard, enumeración, fuga en logs y prod inseguro** | `JWTSettings:Key` y `UseInMemoryDatabase: true` en `appsettings.json`; CORS aceptaba `*.onrender.com` con credenciales; `RequireHttpsMetadata = false` fijo; enumeración de emails; `EmailService` imprimía tokens en consola; `render.yaml` habilitaba seeds y Swagger con JWT 24 h. | Retirado `JWTSettings:Key` de `appsettings.json` (`UseInMemoryDatabase: false`; `true` movido a `appsettings.Development.json`); eliminado wildcard `.onrender.com`; `RequireHttpsMetadata = !IsDevelopment()`; respuestas genéricas en `ForgotPasswordAsync`/`ResetPasswordAsync`; `EmailService` sin `to` ni `body` en logs; `render.yaml` con `SeedInitialData: "false"`, `EnableSwaggerInProduction: "false"` y JWT `60` min. | [appsettings.json](../../../src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.json), [appsettings.Development.json](../../../src/Presentation/RealEstateApp.Presentation.WebApi/appsettings.Development.json), [Program.cs](../../../src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs), [AccountService.cs](../../../src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs), [EmailService.cs](../../../src/Infrastructure/RealEstateApp.Infrastructure.Shared/Services/EmailService.cs), [render.yaml](../../../render.yaml) |
| **0.7** | **Reglas del workspace inactivas** | `architecture_standards.md` y `soul_robert_c_martin.md` no se inyectaban automáticamente por falta de frontmatter. | Se añadió frontmatter `trigger: always_on` en ambos archivos. | [architecture_standards.md](../../../.agents/rules/architecture_standards.md), [soul_robert_c_martin.md](../../../.agents/rules/soul_robert_c_martin.md) |

---

## 3. Suite de Pruebas Añadidas y Actualizadas (TDD Red → Green)

### Nuevas suites creadas en aislamiento por `alr-qa-tdd`
- **Frontend (Vitest — verificadas Red → Green):**
  1. [urlValidator.test.ts](../../../src/Presentation/RealEstateApp.ClientApp/src/tests/urlValidator.test.ts) — `21` casos (proveedores válidos, `javascript:`, `data:`, `vbscript:`, `http://`, ataques de prefijo/sufijo de subdominio, vacíos y `null`/`undefined`).
  2. [EmbedSecurity.test.tsx](../../../src/Presentation/RealEstateApp.ClientApp/src/tests/EmbedSecurity.test.tsx) — `14` casos de renderizado seguro de `VideoPlayer` y `VirtualTourEmbed` (ausencia de `<iframe>` ante payloads maliciosos y presencia de `sandbox` + `referrerpolicy="no-referrer"` en embeds autorizados).
- **Backend (xUnit + FluentAssertions + InMemoryDb):**
  3. [FileValidationServiceTests.cs](../../../tests/RealEstateApp.UnitTests/Services/FileValidationServiceTests.cs) — Triple barrera para `PropertyImage`, `VerificationImage` y `LegalDocument` (incluyendo `BoundedFakeStream` de `5 MB` / `10 MB` exactos y `+1 byte`, ejecutables `MZ`, `.html`/`.svg`/`.js`, PDFs falsificados y MIME spoofing).
  4. [FinancingServiceTests.cs](../../../tests/RealEstateApp.UnitTests/Services/FinancingServiceTests.cs) — Límites fuera de rango (`0`, negativos, `> 100%`, `> 40` años, `500000` años) y cálculos válidos con tasa `0%` y `11.5%`.
  5. [FileStorageServiceTests.cs](../../../tests/RealEstateApp.UnitTests/Services/FileStorageServiceTests.cs) — Path traversal con archivo canario fuera de `_basePath`, `containerName` malicioso y ciclo válido con limpieza en `Dispose()`.
  6. [OfferServiceSecurityTests.cs](../../../tests/RealEstateApp.UnitTests/Services/OfferServiceSecurityTests.cs) — Pruebas sociales en `ApplicationDbContext` (`UseInMemoryDatabase`) validando rechazo de `agent-attacker` e inmutabilidad del estado en BD para `AcceptOffer`, `RejectOffer` y `CounterOffer`, más flujos legítimos de `agent-owner`.
  7. [HubSecurityTests.cs](../../../tests/RealEstateApp.UnitTests/Hubs/HubSecurityTests.cs) — Reflexión confirmando ausencia de `SendNotificationToUser` en `NotificationHub` y pruebas de validación/autorización/UTC en `ChatService.SendMessage`.

### Suites existentes actualizadas (sin eliminar pruebas)
- [OfferServiceTests.cs](../../../tests/RealEstateApp.UnitTests/Services/OfferServiceTests.cs) y [OfferServiceStateTests.cs](../../../tests/RealEstateApp.UnitTests/Services/OfferServiceStateTests.cs) — adaptadas a la nueva firma con `agentUserId` y propiedad dueña.
- [PropertyDocumentServiceTests.cs](../../../tests/RealEstateApp.UnitTests/Services/PropertyDocumentServiceTests.cs) — fixture de happy-path con cabecera real `%PDF-1.4` + 3 casos negativos (`.html`, `.svg`, `.pdf` falsificado).
- [AgentVerificationServiceTests.cs](../../../tests/RealEstateApp.UnitTests/Services/AgentVerificationServiceTests.cs) — caso negativo bloqueando ejecutable `MZ` renombrado a `.jpg` antes de tocar `IFileStorageService`.

---

## 4. Estado del Gate Final (`alr-zeta`)

| Gate | Estado | Detalle |
| :--- | :---: | :--- |
| **Frontend `npm run lint`** | ✅ **Verde** | Exit `0` (`0` errores; `0` warnings en los 5 archivos tocados/nuevos; 115 warnings preexistentes en `services.ts`/contextos diferidos a Fase 3). |
| **Frontend `npm run test`** | ✅ **Verde** | Exit `0` — **`13/13` archivos, `123/123` tests pasando** (`urlValidator.test.ts` 21/21, `EmbedSecurity.test.tsx` 14/14). |
| **Frontend `npm run build`** | ✅ **Verde** | Exit `0` (`tsc && vite build` en `28.56s`). |
| **Auditoría estática Backend / Config (`DELTA` + `ZETA`)** | ✅ **Verde** | `13/13` invariantes confirmados (`0` secretos JWT en `appsettings*.json`, `0` wildcards `.onrender.com`, `0` `SendNotificationToUser`, `0` llamadas 1-arg a `AcceptOffer`/`RejectOffer`, `0` `TODO/FIXME/HACK` nuevos, `render.yaml` endurecido). |
| **Backend `dotnet build` / `dotnet test`** | ⏳ **Pendiente de entorno** | `dotnet` no está instalado en esta máquina. La compilación y ejecución real de la suite xUnit debe validarse en GitHub Actions o tras instalar `.NET 10 SDK` localmente. |

---

## 5. Acciones Operativas antes de Commit / Fase 1

1. **Desarrollo local:** Configurar la clave JWT vía User Secrets (`dotnet user-secrets set "JWTSettings:Key" "<clave-de-32+-caracteres>"`) o variable de entorno `JWTSettings__Key`.
2. **Producción (Render):** Verificar que `FRONTEND_URL` tenga la URL exacta del Static Site (sin el antiguo wildcard `*.onrender.com`).
3. **Validación Backend:** Instalar `.NET 10 SDK` local o abrir PR para que el job `test-backend` de [ci-cd.yml](../../../.github/workflows/ci-cd.yml) ejecute `dotnet build` y `dotnet test` sobre todos los cambios de Fase 0 antes de iniciar la **Fase 1** ([plan.md](../fase-1-arquitectura-turing/plan.md)).
