# Plan Maestro — Elevación de `RealEstateApp` a Grado Turing

> **Proyecto:** `RealEstateApp` (.NET 10 WebAPI + Clean Onion Architecture + React 18 SPA)  
> **Líder / Founder:** Angel Luis Rosario (ALR COMPANY)  
> **Metodología obligatoria:** `ALR COMPANY v5.0` (`alr-alpha` → `alr-qa-tdd` Red → Green / Refactor → `alr-delta` → `alr-zeta`)  
> **Estado base auditado:** `5.4 / 10` (MVP funcional con brechas críticas de seguridad, modelado de dominio, concurrencia, integridad conceptual y observabilidad)  
> **Objetivo:** `>= 9.5 / 10` (Estándar Grado Turing & Producción Enterprise)

---

## 1. Línea Base Cuantificada (Pre-Ejecución)

- **Seguridad (`DELTA`):** BOLA/IDOR crítico en `OffersController`/`OfferService`, `NotificationHub.SendNotificationToUser` expuesto sin autorización de rol, `ChatHub` sin validación de pertenencia ni límite de longitud, `PropertyDocumentService` y `AgentVerificationService` sin Triple Barrera de uploads, path traversal potencial en `FileStorageService.DeleteFileAsync`, XSS por `<iframe src={url}>` sin validación ni `sandbox` en `VideoPlayer.tsx` / `VirtualTourEmbed.tsx`, secreto `JWTSettings:Key` y `UseInMemoryDatabase: true` versionados en `appsettings.json`, wildcard CORS `*.onrender.com` con `.AllowCredentials()`, y `SeedInitialData`/`EnableSwaggerInProduction` activos en `render.yaml`.
- **Crashes / DoS (`Hoare`):** `FinancingService` vulnerable a `DivideByZeroException` / `OverflowException` / agotamiento de memoria si es invocado directamente sin pasar por el controller (`termInYears <= 0` o desmesurado, `downPayment < 0`, `annualRate < 0`).
- **Arquitectura & Integridad (`BETA` / `Dijkstra` / `Brooks` / `Liskov`):** `0` usos de `Result<T>`; 4 patrones distintos de manejo de errores; `0` respuestas `ProblemDetails` RFC 7807 (14 `catch (Exception ex)` filtrando mensajes); 45 `ViewModels` vs 18 `DTOs` en `Core.Application`; `<FrameworkReference Include="Microsoft.AspNetCore.App" />` en `Core.Application.csproj`; `UserManager<IdentityUser>` inyectado en 3 controllers; `OfferRepository` ejecutando lógica de negocio y transacciones propias; `0` tokens de concurrencia optimista (`RowVersion`); SQL exclusivo de SQL Server (`SELECT * FROM sys.columns`) en `DefaultRealEstateData.cs`; `PendingModelChangesWarning` suprimido.
- **Calidad Frontend & Backend (`GAMMA`):** `tsconfig.json` con `"strict": false`, `79` usos de `any`, `115` warnings ESLint, `137` llamadas `console.*`, `LanguageContext.tsx` monolítico (`2,278` líneas), `ApplicationDbContext.cs` monolítico (`882` líneas), `AiSearchService.cs` (`587` líneas, `63` ramas).
- **QA & Observabilidad:** `101` `.Verify()` sobre mocks vs `2` archivos con `InMemoryDatabase` (`18/25` controllers sin tests); `0` tests E2E Playwright; `0` endpoints `/health`; `0` logs estructurados Serilog JSON; `34` `.md` sueltos en raíz + 3 proyectos mock de MCP (`context7/`, `sequential-thinking/`, `shadcn/`) + `WebApp/` huérfana.

---

## 2. Prerrequisito de Entorno

- **Backend (.NET 10 SDK):** Actualmente `dotnet` no está instalado/registrado en el entorno local (`C:\Program Files\dotnet` no contiene el SDK). Antes de iniciar la fase **Red** de cualquier tarea backend en local, se debe instalar `.NET 10 SDK` para habilitar:
  - `dotnet format --verify-no-changes`
  - `dotnet build RealEstateApp.slnx`
  - `dotnet test RealEstateApp.slnx`
- **Frontend (Node.js + npm + Vitest):** Operativo localmente (`npm run lint`, `npm run test`, `npm run build`).

---

## 3. Flujo Obligatorio por Fase (Protocolo v5.0)

Cada fase se ejecutará de forma estrictamente secuencial y aislada:

1. **Aprobación de Contrato y Migraciones (`ALPHA`):** Revisar `.alr/specs/<fase>/plan.md` y obtener confirmación explícita de **Angel Luis Rosario** antes de crear/aplicar cualquier migración de BD.
2. **Fase Red Aislada (`alr-qa-tdd`):** Invocar al subagente `alr-qa-tdd` entregándole **únicamente** el archivo `plan.md` de la fase correspondiente y las firmas de interfaz pública para generar primero los tests fallidos (sin contaminarse con la implementación).
3. **Implementación Mínima & Refactor (`Green` → `Refactor`):** Escribir el código mínimo correcto que haga pasar el 100% de la suite y refactorizar manteniendo complejidad ciclomática `<= 10` y cumplimiento de `domain.md` e `infrastructure-webapi.md`.
4. **Gate Final (`alr-delta` + `alr-zeta`):** Auditar el `git diff` completo y ejecutar los comandos verificables antes de pasar a la siguiente fase.

---

## 4. Índice de Fases y Contratos

| Orden | Fase | Contrato en Disco | Enfoque Principal |
| :---: | :--- | :--- | :--- |
| **0** | **Contención P0** | [`fase-0-contencion-p0/plan.md`](../fase-0-contencion-p0/plan.md) | Seguridad crítica (BOLA, SignalR, Uploads, XSS, CORS, Secretos), crashes financieros y activación de reglas locales `.agents/rules/`. |
| **1** | **Integridad Arquitectónica & Turing** | [`fase-1-arquitectura-turing/plan.md`](../fase-1-arquitectura-turing/plan.md) | `Result<T>`, `ProblemDetails` RFC 7807, Dominio Rico, fronteras `BETA`, concurrencia optimista, paginación y paridad SQL Server/PostgreSQL. |
| **2** | **Auth Moderna, Pagos & Observabilidad** | [`fase-2-auth-pagos-observabilidad/plan.md`](../fase-2-auth-pagos-observabilidad/plan.md) | JWT 15 min + Refresh Token Rotation (`HttpOnly`), Public IDs opacos, Storage Stateless con presigned URLs `<= 15 min`, `Idempotency-Key`, Serilog JSON, `/health` y CI/CD Docker. |
| **3** | **Frontend Estricto, TDD Social & Higiene** | [`fase-3-frontend-tdd-higiene/plan.md`](../fase-3-frontend-tdd-higiene/plan.md) | TypeScript `strict: true`, 0 `any`/warnings/`console.*`, modularización i18n/UI, TDD Anti-Mockitis (`>= 95%` cobertura en core), Playwright E2E, limpieza de raíz y sincronización DX. |

---

## 5. Matriz de Migraciones de Base de Datos (Guardarraíl #1)

> **Regla absoluta:** Ninguna migración destructiva ni alteración de esquema puede generarse o ejecutarse sin confirmación explícita previa de **Angel Luis Rosario**.

| Fase | Migración Planificada | Tablas / Columnas Afectadas | Requiere Confirmación |
| :---: | :--- | :--- | :---: |
| **Fase 0** | *Ninguna* | — | No aplica |
| **Fase 1** | `AddConcurrencyRowVersion` | `Properties.RowVersion`, `Offers.RowVersion` (`IsConcurrencyToken`) | **Sí (Obligatoria)** |
| **Fase 2** | `AddRefreshTokens` | Nueva tabla `RefreshTokens` (hash SHA-256, `ExpiresAt`, `RevokedAt`, `ReplacedByTokenHash`, `UserId`) | **Sí (Obligatoria)** |
| **Fase 2** | `AddPublicIds` | Columna `PublicId` (`Guid`, índice único) en entidades expuestas por API (`Property`, `Offer`, `PropertyAppointment`, `PropertyDocument`, `Commission`, `AgentVerification`, `LeadPipeline`, `SubscriptionPlan`) | **Sí (Obligatoria)** |
| **Fase 2** | `AddIdempotencyKeys` | Nueva tabla `IdempotencyRecords` (`Key`, `UserId`, `Endpoint`, `ResponseStatus`, `ResponseBody`, `CreatedAt`) | **Sí (Obligatoria)** |
| **Fase 3** | *Ninguna* | — | No aplica |

---

## 6. Criterios de Salida Grado Turing (`>= 9.5 / 10`)

- **Seguridad:** `0` vulnerabilidades P0/P1; `0` secretos en Git; Triple Barrera unificada en todo upload; `HttpOnly` Refresh Token Rotation; Public IDs opacos; `Idempotency-Key` en pagos y webhooks.
- **Arquitectura & Principios:** `0` dependencias web en `Core.Domain` y `Core.Application`; `Result<T>` consistente; RFC 7807 `ProblemDetails` en todo error HTTP; complejidad ciclomática `<= 10`; paridad real entre SQL Server y PostgreSQL.
- **Calidad & Pruebas:** `tsconfig.json` con `"strict": true`; `0` errores y `0` warnings en ESLint; `0` `any`; `0` `console.*` en producción; `100%` tests verdes (xUnit + Vitest + Playwright E2E); cobertura `>= 95%` en lógica de pricing, hipoteca, comisiones, ofertas y agenda; pruebas sociales con `InMemoryDb`/SQLite sin abuso de `.Verify()`.
- **Operación & DX:** `/health/live` y `/health/ready` operativos; Serilog JSON estructurado; `docker build` en CI/CD; contenedor no-root con `HEALTHCHECK`; raíz del repositorio limpia y documentación 100% sincronizada.
