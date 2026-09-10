# INFORME DE AUDITORÍA Y SOLUCIONES — RealEstateApp

**Fecha:** 10/09/2026 · **Alcance:** Backend (WebApi .NET 10 + Infrastructure.Persistence) y Frontend (SPA React + TypeScript + Vite)

## 1. Resumen ejecutivo
- Revisión integral previa cerrada: **141/141 assertions e2e** (Client 40, Agent 42, Owner 15, Admin 44), `dotnet test` 79/79, `vitest` 26/26, lint 0 errores, build OK, `React.lazy` con chunk principal de 348.68 kB.
- Se auditaron a fondo backend y frontend por separado en búsqueda de **código sin usar y funciones no implementadas**.
- Hallazgo central: **el "panel del Developer no existe" para el usuario aunque existe en la SPA** — lo ciega una cadena de redirecciones de rol y una creación de cuentas Admin/Developer que por backend fuerza el rol Client.
- Se documentan **7+ hallazgos backend y 6 frontend**, incluyendo stubs que simulan funcionalidad (email, actividad, pago) y un bug de roles crítico.

## 2. Método
- Determinismo: BD en memoria → `run-services.ps1 -Action restart` + `node scripts\dev\seed-live.mjs` + matrices `scripts\dev\matrix-*.cjs`.
- Dos agentes de auditoría en paralelo (backend y frontend): controllers, servicios, endpoints, servicios SPA, contextos y componentes (verificación `file:line`, importaciones reales, endpoints consumidos).
- Código muerto: decisión del usuario = **solo documentar**, no eliminar.

## 3. AUDITORÍA BACKEND

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

## 4. AUDITORÍA FRONTEND

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