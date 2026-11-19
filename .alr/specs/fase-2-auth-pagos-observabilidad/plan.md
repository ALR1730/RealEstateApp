# Contrato ALPHA — Fase 2: Auth Moderna, Pagos Resilientes y Observabilidad Twelve-Factor

> **Fase:** 2 de 3 (`fase-2-auth-pagos-observabilidad`)  
> **Prerrequisito:** Fase 1 cerrada y verificada con `alr-zeta`  
> **Migraciones de BD:** **3 migraciones** (`AddRefreshTokens`, `AddPublicIds`, `AddIdempotencyKeys` — ⚠️ todas requieren confirmación explícita de Angel Luis Rosario)  
> **Roles líderes:** Security Auditor (`DELTA`) + DevOps + Chief Architect (`BETA`) + QA & TDD (`alr-qa-tdd`)

---

## 1. Sub-bloques y Archivos Afectados

### 2.1 JWT Corto (`15 min`) + Refresh Token Rotation (`HttpOnly`)
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Domain & Persistence** | `Entities/RefreshToken.cs` *(Nuevo)*, `ApplicationDbContext.cs`, migración `AddRefreshTokens` | Persistir `TokenHash` (SHA-256), `UserId`, `ExpiresAt`, `CreatedAt`, `RevokedAt`, `ReplacedByTokenHash`, `FamilyId`. |
| **Core.Application & Persistence** | `Interfaces/Services/IAccountService.cs`, `Services/AccountService.cs`, `Controllers/v1/AccountController.cs` | Reducir `DurationInMinutes` por defecto a `15` min. Implementar `POST /api/v1/account/refresh` y `POST /api/v1/account/revoke` con cookie `HttpOnly; Secure; SameSite=Strict`. Al detectar reutilización de un refresh token ya rotado/revocado, revocar toda la familia activa del usuario. |
| **Presentation.ClientApp** | `src/api/axiosClient.ts`, `src/context/AuthContext.tsx` | Guardar Access Token únicamente en memoria React (no en `localStorage`) e implementar interceptor `401` con refresco automático silencioso vía cookie `HttpOnly` (`withCredentials: true`). |

### 2.2 Identificadores Públicos Opacos (Cierre Guardarraíl #6)
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Domain & Persistence** | `Entities/{Property, Offer, PropertyAppointment, PropertyDocument, Commission, AgentVerification, LeadPipeline, SubscriptionPlan}.cs`, migración `AddPublicIds` | Agregar `Guid PublicId` (generado vía `Guid.CreateVersion7()` en .NET 10) con índice único en BD. |
| **Presentation.WebApi & ClientApp** | Controladores v1 + `src/api/services.ts` + `src/types/index.ts` | Migrar contratos y rutas públicas de `{id:int}` a `{publicId:guid}`, manteniendo los `int Id` exclusivamente como claves internas de BD (no expuestos en respuestas ni URLs). |

### 2.3 Almacenamiento Stateless + Presigned URLs (`TTL <= 15 min`)
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Application & Infrastructure.Shared** | `IFileStorageService.cs`, `CloudFileStorageService.cs` *(Nuevo)*, `FileStorageService.cs`, `ServiceRegistration.cs` | Implementar almacenamiento compatible con S3 / Cloudflare R2 / Azure Blob para producción. Documentos privados y archivos KYC se almacenan en bucket privado y se exponen únicamente mediante `GetPresignedReadUrlAsync(key, ttl <= 15 min)`. `FileStorageService` local queda restringido a `Development`. |

### 2.4 Pagos y Webhooks Idempotentes (`infrastructure-webapi.md`)
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Core.Domain & Persistence** | `Entities/IdempotencyRecord.cs` *(Nuevo)*, migración `AddIdempotencyKeys` | Almacenar clave de idempotencia (`Key`, `UserId`, `Route`, `RequestHash`, `StatusCode`, `ResponsePayload`, `ExpiresAt`). |
| **Infrastructure.Shared & WebApi** | `Services/PaymentService.cs`, `Controllers/v1/SubscriptionsController.cs`, nuevo `Controllers/v1/WebhooksController.cs` (`POST /api/v1/webhooks/{provider}`) | Bloquear el simulador `PaymentService` en entornos no-Development (lanzar error de configuración si falta pasarela real en `Production`). Exigir cabecera `Idempotency-Key` en `POST /api/v1/subscriptions/upgrade` y verificar firma criptográfica HMAC + `Idempotency-Key` en webhooks entrantes. |

### 2.5 Observabilidad (`Serilog JSON` + `/health`) & CI/CD Hardening
| Capa | Archivo | Cambio |
| :--- | :--- | :--- |
| **Presentation.WebApi & Infrastructure** | `Program.cs`, `RealEstateApp.Presentation.WebApi.csproj`, `EmailService.cs`, `PaymentService.cs` | Configurar **Serilog** con salida JSON estructurada (`CorrelationId`, `UserId`, `RequestPath`) y reemplazar todos los `Console.WriteLine`. Exponer `GET /health/live` y `GET /health/ready` con verificación de DbContext (`AddDbContextCheck`). |
| **DevOps** | `Dockerfile`, `.github/workflows/ci-cd.yml` | En `Dockerfile`: correr como usuario no-root (`USER app`) y agregar `HEALTHCHECK`. En `ci-cd.yml`: agregar `dotnet format --verify-no-changes`, reemplazar `npm install` por `npm ci`, añadir job `docker-build` (dependiente de `test-backend` y `test-frontend`), y agregar `--fail -sS` a los `curl` de Render. |

---

## 2. Criterios de Aceptación (`GIVEN / WHEN / THEN`)

1. **Refresh Token Rotation & Reutilización:**
   - `GIVEN` un refresh token válido `RT1` ya rotado por `RT2`
   - `WHEN` un atacante o cliente replay intenta usar `RT1` en `POST /api/v1/account/refresh`
   - `THEN` la API responde `401 Unauthorized`, revoca toda la familia de tokens (`RT2` incluido) y registra evento de seguridad en Serilog.
2. **Eliminación de JWT en `localStorage`:**
   - `GIVEN` un inicio de sesión exitoso en la SPA React
   - `WHEN` se inspecciona `window.localStorage`
   - `THEN` no existe ninguna clave `realestate_jwt_token` ni secreto de sesión almacenado en `localStorage`.
3. **Public IDs Opacos:**
   - `GIVEN` una propiedad o documento recién creado
   - `WHEN` se consulta su endpoint REST público
   - `THEN` las URLs y payloads JSON exponen `publicId` (`UUIDv7`), y peticiones con enteros secuenciales (`/properties/1`) ya no exponen registros internos.
4. **Presigned URLs TTL `<= 15 min`:**
   - `GIVEN` un documento legal o imagen KYC almacenada en bucket privado
   - `WHEN` un usuario autorizado solicita su descarga
   - `THEN` recibe una presigned URL temporal con expiración `<= 15` minutos (`900` segundos) y el bucket no tiene acceso anónimo público.
5. **Idempotencia en Pagos y Firma en Webhooks:**
   - `GIVEN` dos peticiones `POST /api/v1/subscriptions/upgrade` con la misma cabecera `Idempotency-Key`
   - `WHEN` ambas llegan al servidor (simultánea o secuencialmente)
   - `THEN` solo se procesa un cobro y la segunda retorna el resultado idempotente original (`200`), mientras que un webhook sin firma válida retorna `401 Unauthorized`.
6. **Observabilidad & CI/CD:**
   - `GIVEN` el contenedor Docker compilado desde `Dockerfile`
   - `WHEN` inicia la aplicación y se consulta `/health/ready`
   - `THEN` responde `200 OK`, el proceso corre bajo usuario no-root (`whoami != root`) y los logs emitidos a stdout son JSON válido.

---

## 3. Riesgos y Confirmaciones Requeridas

- **⚠️ Confirmación explícita requerida (Guardarraíl #1):** `AddRefreshTokens`, `AddPublicIds` (incluyendo backfill de `PublicId` en registros existentes sin perder integridad referencial) y `AddIdempotencyKeys` requieren aprobación explícita de **Angel Luis Rosario** antes de generarse/aplicarse.
- **Coordinación Frontend/Backend:** La migración de `localStorage` → `HttpOnly` cookie requiere validar soporte CORS `AllowCredentials()` con orígenes exactos (`https://...`) y pruebas en navegador real para desarrollo (`http://localhost:5173` ↔ `http://localhost:5196`).
- **Proveedores externos:** En `CloudFileStorageService` y pasarela de pago real se requerirán credenciales/secrets en variables de entorno (R2/S3/Azure Blob, Stripe).
