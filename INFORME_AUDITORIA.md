# INFORME DE AUDITORÍA Y SOLUCIONES — RealEstateApp

**Fecha:** 10/09/2026 · **Alcance:** Backend (WebApi .NET 10 + Infrastructure.Persistence) y Frontend (SPA React + TypeScript + Vite)

## 1. Resumen ejecutivo
- Revisión integral previa cerrada: **141/141 assertions e2e** (Client 40, Agent 42, Owner 15, Admin 44), `dotnet test` 79/79, `vitest` 26/26, lint 0 errores, build OK, `React.lazy` con chunk principal de 348.68 kB.
- Se auditaron a fondo backend y frontend por separado en búsqueda de **código sin usar y funciones no implementadas**.
- Hallazgo central de la 1ª auditoría: **el "panel del Developer no existe" para el usuario aunque existe en la SPA** — lo ciega una cadena de redirecciones de rol y una creación de cuentas Admin/Developer que por backend fuerza el rol Client.
- **Verificación posterior:** el usuario corrigió **12 de 15** hallazgos en teoría; se verificó en código.
- **2ª pasada de auditoría (re-ejecución completa)** tras reportar el usuario que *"el botón de modo no hace nada", "el cambio de moneda no funciona" y "el frontend se superpone en algunas pantallas"*. Nuevos hallazgos críticos: **tema oscuro sin CSS (`dark:` casi inexistente)**, **moneda DOP/USD solo aplica en el mapa** (~90 usos de `formatCurrencyRD` fijo), **superposición por clases Tailwind inválidas + layout sin `min-w-0`**, y **5 hallazgos críticos backend** (contratos SPA↔API rotos en estado de propiedad, KYC y campos de `PropertyDto`; límites de negocio no forzados; 3 enlaces de correo rotos).

## 2. Método
- Determinismo: BD en memoria → `run-services.ps1 -Action restart` + `node scripts\dev\seed-live.mjs` + matrices `scripts\dev\matrix-*.cjs`.
- Dos agentes de auditoría en paralelo (backend y frontend) por **dos pasadas**: controllers, servicios, endpoints, servicios SPA, contextos y componentes (verificación `file:line`, importaciones reales, endpoints consumidos).
- Código muerto: decisión del usuario = **solo documentar**, no eliminar.
- 2ª pasada adicional con foco en 3 síntomas reportados por el usuario (tema, moneda, superposición).

## 3. AUDITORÍA BACKEND (1ª pasada)

### 3.1 CRÍTICO — La SPA no puede crear Administradores/Desarrolladores reales
| Dato | Valor |
|---|---|
| Causa | `ManageUsersPage.tsx:77,79` llama `authService.registerClient({...form, role:'Admin'/'Developer'})` → `POST /account/register-client` (`services.ts:54-57`) |
| Por qué falla | `AccountController.cs:48-59` fija `Roles.Client` a la fuerza; `RegisterRequest` **no tiene** propiedad `Role` |
| Consecuencia | Crear Admin/Developer desde `/admin/users` crea silenciosamente un **Client** — la cuenta Developer nunca puede existir desde la UI |
| Endpoints correctos (existen) | `POST /admin/admins`, `POST /admin/developers` (los usa `CreateAdminPage`/`CreateEditDeveloperPage`) |
| **Solución ofrecida** | `ManageUsersPage` debe llamar a `adminService.createAdmin`/`createDeveloper` con `{firstName,lastName,email,password,phone}` + UX de error. Añadir tests unitarios del controller si aplican |

### 3.2 Endpoints existentes que nadie consume
| Endpoint | Origen |
|---|---|
| `POST /account/register-admin` | controller/AccountController.cs |
| `POST /account/register-developer` | controller/AccountController.cs |
| `GET /reviews/can-review` | `reviewsService` definido en SPA pero sin página que lo llame |
| `GET /subscriptions/admin/plans/{id}` | `subscriptionsService.getAdminPlan` sin uso |

**Solución:** documentar (decisión: no borrar). Nota: `register-admin`/`register-developer` quedan como alternativa servidor-a-servidor.

### 3.3 Métodos de servicio sin llamadas
| Método | Ubicación |
|---|---|
| `RegisterBasicUserAsync` | AccountService |
| `SignOutAsync` | AccountService (WebApi) |
| `DeleteImage` | portfolio/properties service |
| `GetDistinctSectorsAsync` | service + repositorio |
| `GetByIdSaveViewModel` | vista (legacy WebApp) |

**Solución:** documentar; antes de tocar se verifican usos legacy (WebApp).

### 3.4 Stubs — funcionalidad simulada que "no hace nada"
1. **`EmailService.SendAsync` = `Console.WriteLine`** → la confirmación de email y el "olvidé mi contraseña" nunca salen; los clientes no pueden auto-activarse.
   - **Solución adoptada:** SMTP real (opciones en `appsettings`/UserSecrets) + **modo dev**: sin SMTP configurado, se registra en consola.
2. **`GET /account/activity` devuelve datos falsos hardcodeados** (`AccountController.cs:240`); `UserActivityService.GetRecentActivitiesAsync` nunca se usa → la `ActivityPage` muestra demo.
   - **Solución:** conectar el endpoint al servicio real (actividad por usuario) + tests.
3. **`PaymentService` simulado** (pago siempre exitoso) y **sin consumidor** en WebApi; `AgentSubscriptionPage.tsx:45` envía `"mock_pm_card_rd"` hardcodeado con modal que promete "pasarela segura".
   - **Solución adoptada:** pasarela de tarjeta **simulada real** — formulario funcional (Luhn, expiración, CVV) que genera token validado en backend; precios finales correctos; el pago permanece simulado pero con contrato de pasarela explícito.
4. **`WhatsAppService`** registrado en DI sin uso.
   - **Solución:** documentar.
5. **`NotificationHub.SendNotificationToUser`** sin emisor server-side → notificaciones push muertas.
   - **Solución:** emitir eventos reales mínimos (ofertas, citas, comisiones) o desactivar el método y documentarlo. Decisión pendiente de ejecución (recomendado: eventos mínimos + limpieza).
6. **`RegisterUserAsync` no persiste claims `FirstName/LastName`** → admins/devs creados aparecen con nombre en blanco.
   - **Solución:** escribir esos claims en el alta del usuario.

## 4. AUDITORÍA FRONTEND (1ª pasada)

### 4.1 CRÍTICO — Cadena que hace "inexistente" el panel Developer
| Lugar | Problema |
|---|---|
| `LoginPage.tsx:32` | Tras login, rol Developer se redirige a `/admin` → el Admin guard lo bota a `/` |
| `Navbar.tsx:115,214,223,303` | "Mi Panel"/"Mi Perfil" envían a Developer y Owner a `/client` (4 sitios) |
| `ProtectedRoute` | Bien extendido con `allowedRoles[]`; el problema es la dirección de destino |

**Solución ofrecida:** `LoginPage` redirige por rol (`Developer`→`/developer`, `Owner`→`/owner`; Admin/Agent/Client ya OK). `Navbar` resuelve portal+perfil según rol. Sin registro público Developer (decisión) — solo admin los crea.

### 4.2 Funciones de servicio exportadas sin consumir
| Función/objeto | Ubicación |
|---|---|
| `authService.changePassword` | services.ts:83 |
| `propertiesService.getByCode` | services.ts:116 |
| `subscriptionsService.getAdminPlan` | services.ts:367-370 |
| `leadPipelineService.getByStage` | services.ts:626-628 |
| `leadPipelineService.getById` | services.ts:634-637 |
| `simulatorService` (objeto completo) | services.ts:594-603 (la UI calcula en cliente con `calculateFrenchAmortization`) |
| `currencyService` (objeto completo) | services.ts:439-444 (`CurrencyContext` hace `fetch` crudo) |

**Soluciones:** `changePassword` → sección real en los perfiles de cada rol; `currencyService` → consumirlo dentro de `CurrencyContext` (eliminar el `fetch` directo). El resto → documentar (decisión: no borrar).

### 4.3 Pago (frontend)
- `AgentSubscriptionPage.tsx:45` fija `methodToken: "mock_pm_card_rd"`; modal de "pago seguro" sin validación.
- **Solución:** formulario de tarjeta funcional con validación Luhn/expiración/CVV + token validado contra el backend (ver 3.4.3).

### 4.4 Métodos de Context sin consumir
`clearNotifications()`, `removeNotification()` (NotificationContext), `setCurrency()`, `convertToDisplay()` (CurrencyContext), `setTheme()`.
- **Solución:** documentar; `setCurrency`/`convertToDisplay` pasan a usarse al integrar `currencyService`.

### 4.5 NotificationContext
- `NotificationContext.tsx:100` — cierre (closure) obsoleto entre conexiones SignalR → riesgo de eventos duplicados o huérfanos.
- **Solución:** reinicializar el callback cuando cambia la conexión.

### 4.6 Sin hallazgos (ya correcto)
- Sin `onClick` no-op ni `href="#"`; ningún lazy import roto; 0 TODOs; lint/build limpios.

## 5. SOLUCIONES OFRECIDAS (consolidadas con decisiones del usuario)
| # | Hallazgo | Tipo | Solución | Decisión |
|---|---|---|---|---|
| B1 | Crear Admin/Dev force a Client | Bug crítico | Usar `/admin/admins` y `/admin/developers` + claims de nombre | A implementar (F1) |
| B2 | `register-admin`/`register-developer`/`can-review`/`getAdminPlan` | Dead endpoints | Documentar | Documentar |
| B3 | 5 métodos de servicio muertos (incl. `DeleteImage`, `GetDistinctSectorsAsync`) | Dead code | Documentar | Documentar |
| B4 | `EmailService` = console mock | Stub | SMTP real + modo dev | **SMTP real + modo dev** |
| B5 | `/account/activity` datos falsos | Stub | Conectar a servicio real + tests | A implementar (F2) |
| B6 | Pagos simulados siempre OK + token duro | Stub | Pasarela de tarjeta simulada real (Luhn/expiración/CVV + token backend) | **Pasarela simulada real** |
| B7 | `WhatsAppService`/`NotificationHub` sin emisor | Stub | Documentar + eventos mínimos/limpieza | A implementar (F2) |
| B8 | Nombres en blanco (claims ausentes) | Bug menor | Persistir `FirstName/LastName` en el alta | A implementar (F1) |
| F1 | Login→`/admin` y Navbar→`/client` para Developer/Owner | Bug crítico | Redirect y navegación por rol | A implementar (F1) |
| F2 | 7 funciones/objetos de `services.ts` sin uso | Dead code SPA | Consumir `changePassword` y `currencyService`; documentar el resto | Documentar (solo doc) |
| F3 | Métodos de Context sin usar | Dead code SPA | Documentar (algunos pasan a usarse con `currencyService`) | Documentar |
| F4 | Closure obsoleto en `NotificationContext` | Bug menor | Reinicializar callback en reconexión | A implementar (F2) |
| F5 | Errores visuales percibidos | Derivados de stubs | Resueltos con las Fases 1–2 (ActivityPage demo, perfiles en blanco, panel dev) | **Avanzar con lo detectado** |
| — | Registro público Developer | Decisión | Solo admin crea cuentas */admin/developers* | **Sin página pública** |

## 6. PLAN DE EJECUCIÓN APROBADO
- **Fase 1 — Acceso Developer:** LoginPage·Navbar por rol; `ManageUsersPage` por endpoints reales; claims de nombre en `RegisterUserAsync`.
- **Fase 2 — Stubs:** email SMTP+modo dev; `/account/activity` real; pasarela de tarjeta simulada real; notificaciones + closure.
- **Fase 3 — Consumo:** `changePassword` en perfiles; `currencyService` en `CurrencyContext`. **Sin borrar nada** (documentado).
- **Fase 4 — Cierre:** matriz Developer nueva + barrido 4 matrices + re-baseline (dotnet test, vitest, lint, build) + actualizar `INFORME_REVISION_INTEGRAL.md` + este documento.

## 7. Riesgos
- `DeleteImage`, `GetDistinctSectorsAsync`, `GetByIdSaveViewModel` pueden tener uso legacy (WebApp) → solo se documentan, previamente verificados.
- Modo dev sin SMTP seguirá sin emitir correos reales, pero el flujo queda listo para producción.
- Cambiar el redirect de login y la Navbar debe revalidarse por rol en las matrices (permisos 403/200).

## 8. Verificación
Barrido completo de matrices (Client/Agent/Owner/Admin/Developer) + `dotnet test`, `vitest`, lint y build como cierre de cada fase.

---

# 9. VERIFICACIÓN POSTERIOR (10/09/2026) — Estado tras las correcciones del usuario

Comparación uno a uno de los hallazgos 1ª pasada contra el código actual:

| Hallazgo | Estado | Evidencia verificada |
|---|---|---|
| B1 · Crear Admin/Dev force → Client | ✅ Implementado | `ManageUsersPage.tsx:77,79` usa `adminService.createAdmin/createDeveloper` (endpoints reales `/admin/admins`, `/admin/developers`) |
| B4 · `EmailService` console-only | ✅ Implementado | `EmailService.cs:29-54` — SMTP real + fallback dev-mode en consola |
| B5 · `/account/activity` datos falsos | ✅ Implementado | `AccountController.cs:251` llama `UserActivityService.GetRecentActivitiesAsync`; `ActivityPage.tsx:68` consume `authService.getActivity` |
| B8 · Claims nombre vacíos | ✅ Implementado | `AccountService.cs:162-168` persevera claims `FirstName/LastName` al alta |
| F1a · Login redirect por rol | ✅ Implementado | `LoginPage.tsx:30-34` → Developer→`/developer`, Owner→`/owner` |
| F1b · Navbar "Mi Panel" | ✅ Implementado | `Navbar.tsx:28` `panelUrl` por rol (5 roles) |
| F2 · `changePassword` sin uso | ✅ Implementado | Consumido en `ClientProfilePage.tsx:97` |
| F2 · `currencyService` con fetch crudo | ✅ Implementado | `CurrencyContext.tsx:2,30` usa `currencyService.getRates()` |
| F4 · Closure `NotificationContext` | ✅ Implementado | `NotificationContext.tsx:30-34` usa `tokenRef` + cleanup de conexiones |
| B2/B3/F3 · Código muerto | 📄 Solo documentado | Se mantiene presente (decisión del usuario) |
| F1c · Navbar "Mi Perfil" | ⚠️ **Parcial** | `Navbar.tsx:29` → Developer/Owner aún apuntan a `/client/profile` (solo Admin/Agent/Client correctos) |
| B6 · Pasarela de pago | ❌ **Pendiente** | `AgentSubscriptionPage.tsx:45` sigue con `"mock_pm_card_rd"`; `PaymentService.cs` sigue "pago siempre exitoso" |
| B7 · Push de notificaciones | ❌ **Pendiente** | `NotificationHub.SendNotificationToUser` sigue sin emisor server-side |

**12 corregidos ✅ · 1 parcial ⚠️ · 2 pendientes ❌**

---

# 10. RE-AUDITORÍA COMPLETA — 2ª pasada (10/09/2026)

Lanzada por los 3 síntomas reportados por el usuario: *"el botón de cambiar de modo no me hace nada (se queda siempre en blanco)"*, *"el cambio de moneda no funciona"* y *"el frontend se está superponiendo en algunas pantallas"*. Se reelanzaron ambas auditorías de extremo a extremo.

## 10.1 FRONTEND — 3 bugs confirmados (los que reportó el usuario)

### Bug A · El toggle de tema "no hace nada"
El mecanismo funcional existe y funciona (el ícono cambia), **pero no hay ni una sola variante `dark:`** en los componentes → la pantalla queda idéntica al alternar. "Se queda en blanco" = no se observa ningún cambio visual.

| Evidencia | Archivo:Línea |
|---|---|
| `darkMode: 'class'` correcto | `tailwind.config.js:3` |
| Contexto aplica/remueve `.dark` en `<html>` + persiste | `src/context/ThemeContext.tsx:20-39` |
| Botón alterna estado e ícono Moon/Sun | `src/components/common/ThemeToggle.tsx:10-14` |
| **`dark:` aparece 1 sola vez en todo `src/`** (solo color del ícono) | `src/components/common/ThemeToggle.tsx:11` |
| Bloques claros hardcodeados que anulan `.dark` | `index.html:16`, `src/App.tsx:87,100` (`bg-slate-50 text-slate-900`) |
| Reglas `.dark body` casi vacías | `src/index.css:10-13` |

**Conclusión:** el toggle funciona a nivel de estado, pero la app no define estilos oscuros → aparentemente "no hace nada". El test `ThemeContext.test.tsx` confirma que estado/clase funcionan.

### Bug B · El cambio de moneda DOP↔USD no se ve
Mismo patrón: el switch funciona mecánicamente, **pero ~91 usos de `formatCurrencyRD` muestran RD$ fijo** e ignoran el contexto.

| Evidencia | Archivo:Línea |
|---|---|
| `formatPrice` (respeta moneda) se usa SOLO en 2 archivos | `PropertyMapCard.tsx:13,33` y `MapMarker.tsx:12,29` |
| **91 usos** de `formatCurrencyRD` (RD$ fijo) | `src/utils/formatters.ts:7-17`; usado en `PropertyCard.tsx:4,124`, `PropertyDetailPage.tsx:5,266,352`, MortgageCalculator, AgentDashboard:6, ClientDashboard:6, LeadPipelinePage:4, ManageAllPropertiesPage:4, ReceivedOffersPage:85, etc. |
| Contexto expone `formatPrice`/`convertToDisplay` con `DEFAULT_RATE=58.5` | `CurrencyContext.tsx:18,54-66,87` |

**Conclusión:** al cambiar a USD solo cambian la tarjeta del mapa y el marcador; catálogo, detalle, dashboards y ofertas siguen en RD$.

### Bug C · Superposición de pantallas
| Evidencia | Archivo:Línea |
|---|---|
| `DashboardLayout`: `<main>` sin `min-w-0` → el hijo flex con anchos mínimos desborda | `src/App.tsx:102-104` |
| Sidebar fijo `w-64` sin versión responsive (no colapsa en móvil) | `src/components/common/Sidebar.tsx:44` |
| `body overflow-x-hidden` recorta el contenido desbordado | `src/index.css:7` |
| Navbar `sticky z-40` + dropdowns `z-50` compiten con el Modal `z-50` | `Navbar.tsx:46,165,206` y `Modal.tsx:36` |
| **Clases Tailwind v4 inválidas en proyecto v3** (ignoradas en silencio): | |
| `aspect-4/3` (no-op) | `PropertyCard.tsx:72`, `ManageVerificationsPage.tsx:208,219`, `AgentVerificationPage.tsx:145,174` |
| `aspect-16/9` (no-op) | `PropertyDetailPage.tsx:182` |
| `shadow-xs`/`shadow-2xs` (100+ usos; no definidas) | ej. `MortgagePage.tsx:29,39,49` |
| `rounded-br-xs`/`rounded-bl-xs` (no-op) | `ChatBox.tsx:160-161` |

**Conclusión:** los contenedores de imagen (`aspect-4/3`/`16/9` no-op) colapsan y provocan overlaps; faltan sombras; en pantallas estrechas el sidebar fijo + tablas anchas (p. ej. `LeadPipelinePage` con `min-w-[1200px]`) desbordan el layout sin scroll horizontal.

## 10.2 BACKEND — 5 hallazgos críticos nuevos

### C1 · Estado de propiedad: idioma roto (Español vs Inglés)
El backend guarda/devuelve `"Disponible"/"Reservada"/"Vendida"` (`Constants/PropertyStatus.cs:9-11`, `Entities/Property.cs:19`), pero la SPA compara `'Sold' | 'Available' | 'Reserved'` (`types/index.ts:99`, `PropertyCard.tsx:66`, `ComparePage.tsx:127,134`, `PropertyDetailPage.tsx:138`, `AgentDashboard.tsx:71-72`, `OwnerDashboard.tsx:52-54`). Sin capa de normalización → **los badges, filtros y detección de "vendida/reservada" están muertos en la UI**.

### C2 · `Property` (SPA) vs `PropertyDto` (API) — ~6 campos en desacuerdo
La API devuelve `rooms`, `sizeInMeters`, `videoUrl`, `tour360Url` e `images` como `string[]` (`PropertyDto.cs:40,46,59,63,78`); la SPA lee `bedrooms`, `landSizeMeters`, `videoTourUrl`, `virtualTour360Url` e `images[].imageUrl` (`types/index.ts:90-91,107-108`).
- **Imágenes:** `PropertyCard.tsx:35-37`, `PropertyMapCard.tsx:16-17`, `MapMarker.tsx:15-16`, `ComparePage.tsx:45-46`, `MyPropertiesPage.tsx:99-100`, `OwnerDashboard.tsx:136-137` hacen `images[0].imageUrl` sobre `string[]` → `undefined` → **todas las fotos caen a un demo de Unsplash**.
- **`bedrooms`/`landSizeMeters`:** en blanco en `PropertyCard.tsx:164,172`, `PropertyDetailPage.tsx:280,290`, `ComparePage.tsx:78,96`, `MapMarker.tsx:37`, `MyPropertiesPage.tsx:121`, `OwnerDashboard.tsx:166,172`; el sort por `bedrooms` da NaN (`PropertiesCatalogPage.tsx:187`).
- **Tours 360/video:** checks en `PropertyDetailPage.tsx:323-329` nunca se renderizan; prefill de edición vacío (`CreateEditPropertyPage.tsx:110-127`).
- Nota: la **escritura** sí está bien (`CreateEditPropertyPage.tsx:189-202` postea `Rooms/SizeInMeters/VideoUrl/Tour360Url`).

### C3 · Verificación KYC: también Español vs Inglés
Backend devuelve `"Pendiente"/"Aprobado"/"Rechazado"` (`Constants/VerificationStatus.cs:5-7`; `AgentVerificationService.cs:148`); la SPA compara `'Pending'/'Rejected'` (`AgentVerificationPage.tsx:66-67`, `ManageVerificationsPage.tsx:95,153-164`) → **el flujo de KYC no matchea en la UI**. (Contraste: comisiones y leads SÍ están alineados, ambos en español.)

### C4 · Límites de negocio no forzados por la API
- **Límite de destacados:** `CanAgentFeaturePropertyAsync` solo lo usa el WebApp legacy; el API `PropertiesController.ToggleFeaturedAsync` (`PropertiesController.cs:203-217`) → `PropertyService.ToggleFeaturedAsync` **no valida nada**. Un agente puede destacar infinitas (plan "Gratuito" tiene `MaxFeaturedProperties = 0`).
- **Conteos por agente/propietario** incluyen Vendidas (`PropertyRepository.GetByAgentIdAsync` sin filtrar por estado).
- **Rama "Owner" muerta:** los seeds solo definen Gratuito/Pro/Premium (`Seeds/DefaultSubscriptionPlans.cs`); `PropertyService.cs:138` (`PlanName == "Owner"`) nunca matchea → el límite del Owner cae por conteo (`OwnersController.cs:20,69`).

### C5 · Tres enlaces de correo rotos
| Enlace | Línea | Problema |
|---|---|---|
| Confirm-email | `AccountService.cs:176-181` | Genera `/confirm-email?` sin origen API; la ruta real es `/api/v1/account/confirm-email` |
| Forgot-password | `AccountService.cs:516-518` | Apunta al legacy MVC `/Account/ResetPassword...`; la ruta SPA es `/reset-password` |
| Saved-search alerts | `SavedSearchService.cs:202` | Hardcodea `http://localhost:5080/Home/Details/{id}` (WebApp legacy); la API real es `/api/v1/properties/{id}` |

## 10.3 Código muerto confirmado adicional (2ª pasada)

**Backend / endpoints sin consumidores (verificado contra `services.ts`):**
- `POST /properties/code/{code}` (solo `propertiesService.getByCode`, sin páginas), `GET /subscriptions/admin/plans/{planId}`, `GET /agents/{id}/properties` (`AgentsController.cs:60`), `PATCH /agents/{id}/change-status` (`AgentsController.cs:79`), `GET /leads/stage/{stage}`, `GET /leads/{id}`, `PATCH /leads/{id}/sort-order`, `POST /simulator/calculate`, `GET /reviews/can-review`, `GET /properties/sectors` (solo WebApp legacy).
- `AppointmentService.CompleteAppointmentAsync` (`AppointmentService.cs:177`): solo la usa el WebApp legacy → **ningún endpoint WebApi completa citas** → el estado `Completed` es inalcanzable vía API.
- DI sin resolver: `IPaymentService` (`Shared/ServiceRegistration.cs:15`), `IWhatsAppService` (`:16`), `IFinancingService` (solo consumido por el muerto `SimulatorController`).

**Frontend:**
- **`ReassignModal` completo** (nunca importado; ninguna página lo renderiza) + el stub roto `adminService.reassignProperties('','')` (`ReassignModal.tsx:14,44`). Correcto llamado existe en `ManageAgentsPage.tsx:68` / `ManageAllPropertiesPage.tsx:52`.
- `simulatorService`, `propertiesService.getByCode`, `subscriptionsService.getAdminPlan`, `leadPipelineService.getByStage/getById/updateSortOrder`, `reviewsService.canReview`.
- Import sin uso: `useTheme` en `Navbar.tsx:6`.

## 10.4 Contratos SPA↔API rotos (2ª pasada)
- Estado de propiedad (C1), KYC (C3), campos de `PropertyDto` (C2).
- **OK (alineados):** estados de oferta (PascalCase via `ToString()`), citas (con `JsonStringEnumConverter`), contraoferta (`OffersController.cs:192-196`), cancelación de citas (`[FromBody] string?`).
- **Bugs de comportamiento:** `ReassignModal.tsx:44` envía códigos vacíos → no-op silencioso; `OfferService.cs:64` calcula la comisión contraoferta con `MontoOfertado` antiguo en vez de la contraoferta aceptada; `ChatService.cs:51` marca todo mensaje como `IsWhatsApp = true`; ``JWT fallback secret` hardcodeado `Program.cs:134`; `RequireHttpsMetadata=false` `Program.cs:145`; rate-limit solo aplicado a `authenticate` (`AccountController.cs:30`); activity-logs con URLs legacy (`AppointmentService.cs:104`, `OfferService`).

## 10.5 Notas positivas (2ª pasada)
- Oferta atómica correcta (`OfferRepository.AcceptOfferTransactionAsync`), comisiones auto-creadas al aceptar, validación de reseñas/favoritos con de-dupe, calculadora de hipoteca/`CurrencyService` reales, CORS con allowlist, HSTS, 79 tests backend en 15 archivos.

---

# 11. PRIORIDADES DE CORRECCIÓN RECOMENDADAS (pendientes de aplicar)

1. **Contratos SPA↔API (C1, C2, C3)** — normalizar estados (una sola fuente: enums inglés con display español), regenerar el tipo `Property` desde `PropertyDto` o capa de mapeo DTO. *Impacto: imágenes reales no se ven en ninguna tarjeta.*
2. **Tema oscuro (Bug A)** — definir variantes `dark:`/reemplazar `bg-slate-*.` hardcodeados en `index.html`, `App.tsx`, `index.css`.
3. **Moneda (Bug B)** — migrar los ~91 `formatCurrencyRD` → `formatPrice`(contexto) en todas las pantallas de precios.
4. **Superposición (Bug C)** — `min-w-0` en `<main>`, sidebar responsive, corregir `aspect-4/3`→`aspect-[4/3]`, `aspect-16/9`→`aspect-[16/9]`, y eliminar/redefinir `shadow-xs`/`shadow-2xs`.
5. **Límites de negocio (C4)** — forzar el límite de destacados en `PropertyService.ToggleFeaturedAsync`.
6. **Enlaces de correo (C5) + flujo de pago (B6) + push notificaciones (B7)** — rutas SPA correctas, pasarela simulada real, emisores reales.
7. **Código muerto** — eliminar o integrar `ReassignModal`, endpoints sin consumir, y `formatCurrencyRD` residual.

> **Estado:** pendiente de aprobación. Únicamente se ha actualizado la documentación en esta pasada; no se ha modificado código fuente.