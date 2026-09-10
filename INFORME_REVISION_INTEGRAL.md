# INFORME DE REVISIÓN INTEGRAL — RealEstateApp

**Alcance:** Revisión técnica en vivo de la aplicación (SPA React + WebApi .NET 10), verificación funcional por rol con matrices e2e, corrección de bugs reales, desarrollo del portal Developer y optimización de rendimiento.

**Actualización 10/09/2026:** Se añadió la **2ª auditoría completa** (sección 8) — re-ejecución de backend y frontend tras reportar el usuario problemas de tema oscuro, moneda y superposición de pantallas, incluyendo la verificación de cuáles hallazgos de la 1ª pasada ya fueron corregidos.

**Método:** Verificación en vivo contra la API (`http://localhost:5196`) con scripts determinísticos por rol → ante fallo real, corrección del código → re-verificación → repetición hasta matriz en verde.

---

## 1. RESULTADOS FUNCIONALES POR ROL (matrices e2e)

| Rol | Verificaciones | Estado |
|---|---|---|
| **Client** | 40 | ✅ VERDE |
| **Agent** | 42 | ✅ VERDE |
| **Owner** | 15 | ✅ VERDE |
| **Admin** | 44 | ✅ VERDE |
| **TOTAL** | **141** | **100% VERDE** |

Cobertura por rol (no exhaustivo):
- **Client:** autenticación, perfil multipart, catálogo público, favoritos, saved searches (CRUD), ofertas (crear/counter/aceptar), citas, compare, reseña post-transacción, estado de propiedad Vendida (404 público por diseño), links de pago, leads, valuación.
- **Agent:** CRUD propiedad multipart, price-history, featured, documentos legales, KYC submit, citas, pipeline de leads, AVM, ofertas recibidas + aceptación directa, comisiones (listado/resumen), verificaciones de permisos **Admin-only** (reassign y pago → 403 correcto).
- **Owner:** límite de 2 propiedades activas (3ª rechazada), liberación de slot y re-publicación, actualización de precio, price-history, eliminación, estructura de `my-properties`.
- **Admin:** KPIs, aprobación **y rechazo** de KYC, comisiones globales + pago, reassign entre agentes, toggle-status de agentes, creación de admins/desarrolladores reales, CRUD de catálogos (propertytypes/saletypes/improvements), reviews globales, planes de suscripción, acuerdos de autorización.

---

## 2. BUGS REALES ENCONTRADOS Y CORREGIDOS (con verificación en vivo)

| # | Bug | Impacto | Fix | Tests |
|---|---|---|---|---|
| 1 | Oferta con counter-ofrecimiento: `AcceptCounterOffer` no volvía a `Pending`, rompía la transacción atómica | Aceptar contraoferta fallaba en runtime | Set `Status = Pending` + `UpdateAsync` antes de `AcceptOfferTransactionAsync` | +2 |
| 2 | AVM devolvía **500** al valuar | No había mapa `Property → ComparablePropertyDto` | `CreateMap` faltante en `GeneralProfile` | — |
| 3 | `ValidationException` → **500** genérico | Respuestas de negocio ilegibles, sin status correcto | `ApiGlobalExceptionFilter`: `ValidationException→400`, `NotFoundException→404` | +3 |
| 4 | `DELETE /owners/properties/{id}` → **500** | Conflicto de tracking EF (`AsNoTracking` + re-consulta trackeada) | `PropertyService.Delete` usa las imágenes ya cargadas | +2 |
| 5 | `POST /propertytypes`, `/saletypes`, `/improvements` → **500** "No route matches the supplied values" | **Rompía la SPA de gestión de catálogos** | `CreatedAtAction(nameof(GetByIdAsync))` → `Ok(dto)` | +2 |
| 6 | Crear Admin/Developer desde la SPA → **400** | Los formularios no enviaban `FirstName/LastName/ConfirmPassword` | Campos añadidos a ambos formularios y payloads | — |
| 7 | `confirm-email` devolvía **200** siempre, aún con token inválido | Éxito falso en la confirmación de cuenta | `NotFoundException`/`ValidationException` → 404/400 reales (filtro global); `ConfirmEmailPage` reescrita para llamar a la API; enlace del email apunta a la SPA | +3 |

**Total de nuevos tests backend: +12** → `dotnet test`: **79/79 PASADOS** (cero fallos).

Herramienta clave del hallazgo: matrices por rol como **contratos vivos** — cada bug fue detectado gracias a una verificación dirigida, corriregido en código y re-verificado hasta verde.

---

## 3. NUEVO: PORTAL DEVELOPER (SPA)

El rol **Developer no tenía ninguna interfaz**. Se construyó un portal básico:

- **Rutas SPA:** `/developer`, `/developer/property-types`, `/developer/sale-types`, `/developer/improvements`, `/developer/profile` (todas protegidas por rol `Developer`).
- **Menú Sidebar dedicado** (etiqueta de rol "Desarrollador") + **Dashboard con referencia rápida de endpoints y CRUDs**.
- **CRUD de catálogos reutilizados** (mismas páginas de mantenimiento, correctas para `Admin,Developer` según la API).
- Guarda de rutas extendida: `ProtectedRoute` ahora soporta `allowedRoles[]`.

**Verificado en vivo:** crear developer (admin) → login → `POST /propertytypes` = **200** → `GET /admin/dashboard-kpis` = **403** (permisos correctos).

---

## 4. RENDIMIENTO: REACT.LAZY (CODE SPLITTING)

- Antes: bundle único **1,593.31 kB** (warning >500 kB).
- Después: **todas** las páginas cargan con `React.lazy` + `Suspense`.
- Resultado: chunk principal **348.68 kB**, chunks por ruta de 14–269 kB; sin warning de tamaño en el build.

---

## 5. RESULTADOS DE CALIDAD (re-baseline final)

| Verificación | Comando | Resultado |
|---|---|---|
| Backend tests | `dotnet test` | **79/79** ✅ |
| Frontend tests | `npm run test` (vitest) | **26/26** ✅ |
| Lint | `npm run lint` | **0 errores** ✅ |
| Build | `npm run build` | **OK** ✅ |

---

## 6. CÓMO REPRODUCIR

```powershell
# 1) Levantar API + SPA
powershell -ExecutionPolicy Bypass -File scripts\dev\run-services.ps1 -Action start

# 2) Sembrar datos de la última verificación
node scripts\dev\seed-live.mjs

# 3) Ejecutar cada matriz por rol (desde scripts\dev)
node scripts\dev\matrix-client.cjs   # 40 checks
node scripts\dev\matrix-agent.cjs    # 42 checks
node scripts\dev\matrix-owner.cjs    # 15 checks
node scripts\dev\matrix-admin.cjs    # 44 checks
```

Credenciales demo:
- `admin@realestate.com` / `Admin123!`
- `agent@realestate.com` / `Agent123!` · `agent2@realestate.com` / `Agent123!`
- `client@realestate.com` / `Client123!` · `client2@realestate.com` / `Client123!`
- `owner@realestate.com` / `Owner123!`

---

## 7. ARCHIVOS MODIFICADOS (resumen)

**Backend (fixes):**
- `src/Core/RealEstateApp.Core.Application/Services/OfferService.cs` — fix counter-offer.
- `src/Core/RealEstateApp.Core.Application/Mappings/GeneralProfile.cs` — mapa AVM.
- `src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs` — fix delete (tracking EF).
- `src/Presentation/RealEstateApp.Presentation.WebApi/Filters/ApiGlobalExceptionFilter.cs` — nuevo.
- `src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/{PropertyTypes,SaleTypes,Improvements}Controller.cs` — fix 500 en POST.
- `src/Presentation/RealEstateApp.Presentation.WebApi/Program.cs` — registro del filtro.
- `src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Services/AccountService.cs` — confirm-email real (404/400) + enlace SPA.

**Frontend:**
- `src/api/services.ts` — `confirmEmail`, consistencia de endpoints.
- `src/pages/auth/ConfirmEmailPage.tsx` — reescrita (llama a la API).
- `src/pages/admin/{CreateAdminPage,CreateEditDeveloperPage}.tsx` — fix 400 en creación.
- `src/pages/developer/DeveloperDashboard.tsx` — **nuevo**.
- `src/App.tsx` — rutas Developer, `allowedRoles`, **React.lazy** (57 páginas).
- `src/components/common/Sidebar.tsx` — menú/menú Developer.

**Pruebas:**
- `tests/.../OfferServiceTests.cs` (+2), `ApiGlobalExceptionFilterTests.cs` (+3), `PropertyServiceTests.cs` (+2), `PropertyTypesControllerTests.cs` (+2), `AccountControllerTests.cs` (+3).

**Herramientas de verificación:** `scripts/dev/matrix-{client,agent,owner,admin}.cjs` + reportes `*-report.txt`.

---

## 8. 2ª AUDITORÍA COMPLETA (10/09/2026) — HALLAZGOS DE LA RE-EJECUCIÓN

Re-ejecutadas ambas auditorías (backend y frontend) por los síntomas reportados por el usuario:
*"el botón de cambiar de modo no me hace nada / se queda siempre en blanco"*, *"el cambio de moneda no funciona"* y *"el frontend se está superponiendo en algunas pantallas"*.

### 8.1 Verificación de la 1ª pasada (estado actual)
| Hallazgo 1ª pasada | Estado |
|---|---|
| B1 Crear Admin/Dev force→Client | ✅ Implementado — `ManageUsersPage.tsx:77,79` |
| B4 Email console-mock | ✅ Implementado — `EmailService.cs:29-54` SMTP + dev-mode |
| B5 `/account/activity` datos falsos | ✅ Implementado — `AccountController.cs:251` + `ActivityPage.tsx:68` |
| B8 Claims nombre vacíos | ✅ Implementado — `AccountService.cs:162-168` |
| F1a Login redirect por rol / F1b Navbar "Mi Panel" | ✅ Implementado — `LoginPage.tsx:30-34`, `Navbar.tsx:28` |
| F2 `changePassword` / `currencyService` | ✅ Implementado — `ClientProfilePage.tsx:97`, `CurrencyContext.tsx:2,30` |
| F4 Closure `NotificationContext` | ✅ Implementado — `NotificationContext.tsx:30-34` (tokenRef + cleanup) |
| F1c Navbar "Mi Perfil" (Dev/Owner) | ⚠️ Parcial — `Navbar.tsx:29` aún → `/client/profile` |
| B6 Pasarela de pago (`mock_pm_card_rd`) | ❌ Pendiente — `AgentSubscriptionPage.tsx:45`, `PaymentService.cs` |
| B7 Push notificaciones (`SendNotificationToUser`) | ❌ Pendiente — sin emisor server-side |
| B2/B3/F3 Código muerto | 📄 Solo documentado (decisión del usuario) |

### 8.2 Bugs nuevos confirmados (USUARIO)
- **Bug A — Tema oscuro "no hace nada":** el toggle funciona (`ThemeToggle.tsx:10-14`, `ThemeContext.tsx:20-39`) pero `dark:` aparece **1 sola vez** en todo `src/` (`ThemeToggle.tsx:11`); bloques claros hardcodeados en `index.html:16`, `App.tsx:87,100`; `index.css:10-13` vacío. Sin estilos oscuros → la UI no cambia.
- **Bug B — Moneda DOP↔USD solo afecta al mapa:** `formatPrice` (contexto) usado solo en `PropertyMapCard.tsx:13,33` y `MapMarker.tsx:12,29`; **~91 usos** de `formatCurrencyRD` (RD$ fijo) en catálogo, detalle, dashboards y ofertas (`formatters.ts:7-17`).
- **Bug C — Superposición de pantallas:** `<main>` sin `min-w-0` (`App.tsx:102-104`), sidebar fijo `w-64` no responsive (`Sidebar.tsx:44`), `body overflow-x-hidden` (`index.css:7`), z-index Navbar/Modal (`Navbar.tsx:46,165,206`, `Modal.tsx:36`), y clases Tailwind v4 inválidas en v3 ignoradas: `aspect-4/3` (`PropertyCard.tsx:72`…), `aspect-16/9` (`PropertyDetailPage.tsx:182`), `shadow-xs`/`shadow-2xs` (100+ usos), `rounded-br-xs`/`rounded-bl-xs` (`ChatBox.tsx:160-161`).

### 8.3 Hallazgos backend críticos nuevos
- **C1 — Estado de propiedad Español vs Inglés:** backend `"Disponible/Reservada/Vendida"` (`PropertyStatus.cs:9-11`); SPA compara `'Sold'/'Available'/'Reserved'` (`types/index.ts:99`, `PropertyCard.tsx:66`, `ComparePage.tsx:127,134`, `PropertyDetailPage.tsx:138`, `AgentDashboard.tsx:71-72`, `OwnerDashboard.tsx:52-54`) → badges/filtros muertos.
- **C2 — `Property` (SPA) vs `PropertyDto` (API), ~6 campos en desacuerdo:** la API envía `rooms/sizeInMeters/videoUrl/tour360Url` e `images: string[]` (`PropertyDto.cs:40,46,59,63,78`); la SPA lee `bedrooms/landSizeMeters/videoTourUrl/virtualTour360Url` e `images[].imageUrl` → **todas las fotos caen al demo de Unsplash**, campos en blanco, sort NaN, tours nunca renderizan.
- **C3 — KYC Español vs Inglés:** `"Pendiente/Aprobado/Rechazado"` (`VerificationStatus.cs:5-7`) vs `'Pending'/'Rejected'` SPA (`AgentVerificationPage.tsx:66-67`, `ManageVerificationsPage.tsx:95,153-164`).
- **C4 — Límites de negocio no forzados en la API:** `ToggleFeaturedAsync` no valida el límite de destacados (`PropertiesController.cs:203-217`); conteos incluyen Vendidas; rama `PlanName=="Owner"` muerta (`PropertyService.cs:138`).
- **C5 — 3 enlaces de correo rotos:** confirm-email (`AccountService.cs:176-181`), forgot-password → `/Account/ResetPassword` legacy (`AccountService.cs:516-518`), saved-search → `http://localhost:5080/Home/Details/{id}` hardcodeado (`SavedSearchService.cs:202`).

### 8.4 Código muerto adicional confirmado
- **Backend:** `POST /properties/code/{code}`, `GET /subscriptions/admin/plans/{planId}`, `GET /agents/{id}/properties`, `PATCH /agents/{id}/change-status`, `GET /leads/stage/{stage}`, `GET /leads/{id}`, `PATCH /leads/{id}/sort-order`, `POST /simulator/calculate`, `GET /reviews/can-review`, `GetDistinctSectorsAsync` (WebApp-only), `CompleteAppointmentAsync` (WebApp-only → estado citas `Completed` inalcanzable vía API). DI sin resolver: `IPaymentService`, `IWhatsAppService`, `IFinancingService`.
- **Frontend:** `ReassignModal` completo nunca importado + stub `reassignProperties('','')` (`ReassignModal.tsx:14,44`); `simulatorService`, `getByCode`, `getAdminPlan`, `getByStage/getById/updateSortOrder`, `canReview`; import `useTheme` sin uso (`Navbar.tsx:6`).
- **Bugs de comportamiento:** comisión de contraoferta con `MontoOfertado` viejo (`OfferService.cs:64`); `ChatService.cs:51` marca todo mensaje `IsWhatsApp=true`; JWT fallback secret hardcodeado (`Program.cs:134`); `RequireHttpsMetadata=false` (`Program.cs:145`); rate-limit solo en `authenticate`.

### 8.5 Prioridades de corrección recomendadas (PENDIENTES de aplicar)
1. Contratos SPA↔API (C1, C2, C3) — normalizar estados + regenerar tipo `Property`/DTO mapper. *Las imágenes reales no se ven hoy.*
2. Tema oscuro (Bug A) — variantes `dark:` / reemplazar `bg-slate-*` hardcodeados.
3. Moneda (Bug B) — migrar ~91 `formatCurrencyRD` → `formatPrice`.
4. Superposición (Bug C) — `min-w-0`, sidebar responsive, `aspect-[4/3]`/`aspect-[16/9]`, revisar `shadow-xs`.
5. Límites de negocio (C4) en `PropertyService.ToggleFeaturedAsync`.
6. Enlaces de correo (C5), pasarela de pago (B6), notificaciones (B7).

> **Estado:** documentación actualizada en esta pasada; **no se ha modificado código fuente**. Detalle completo en `INFORME_AUDITORIA.md` (secciones 9, 10 y 11).