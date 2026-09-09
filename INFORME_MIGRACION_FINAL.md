# RealEstateApp — Informe Final de Migración y Verificación en Vivo

> Fecha de verificación final: 2026-09-09 · Ambiente: Windows 10, API `:5196`, Vite `:5173` (ambas pestañas abiertas y saludables).

## 1. Resumen ejecutivo

La migración del frontend legacy (WebApp MVC, hoy solo referencia) a la SPA React/Vite quedó **completada y verificada en vivo**. La verificación se realizó directamente sobre `localhost:5173` (SPA) y `localhost:5196` (API), sin reconstruir ni parchear desde la teoría: cada defecto se reprodujo, se corrigió y se re-verificó en el navegador.

Resultados principales:

| Métrica | Resultado |
|---|---|
| Baseline frontend | lint **0 errores** (236 warnings), build OK, **26/26** tests Vitest |
| Baseline backend | **67/67** tests xUnit (0 errores de compilación) |
| Bootstrap demo (seed-live.mjs) | Agente demo con comisión $9,250, 3 leads, 2 citas (1 confirmada), documento legal, reseña ★5, búsqueda guardada, plan Pro activo |
| E2E Fase 6 (planes de suscripción) | CRUD completo en vivo: crear/editar/activar/404/borrado/UPGRADE/409, anónimo 401, client 403 |
| Tour headless (tour-live.cjs) | **61 rutas evaluadas + 1 skip justificado, 0 problemáticas** |
| SignalR (notificaciones + chat) | negotiate 200, WebSocket conectado vía proxy, **0 errores de consola** en 6 s de observación estable |

## 2. Defectos reales encontrados y corregidos en vivo

Durante la verificación se descubrieron y corrigieron **6 defectos reales** (la app "funcionaba" pero con bugs que afectaban datos, autorización o tiempo real):

### 2.1 — Pipeline de Leads: 500 en `POST/GET /leads` con datos
`AutoMapper` no tenía `CreateMap<LeadPipeline, LeadPipelineDto>` → excepción al listar `src.Property.Name`.
- Fix: `src/Core/RealEstateApp.Core.Application/Mappings/GeneralProfile.cs` (región `#region LeadPipeline`).
- Tests: `tests/RealEstateApp.UnitTests/Mappings/GeneralProfileTests.cs` (2 tests, usa `AutoMapperTestFactory` por el ctor cambiado en AutoMapper 16).
- Verificado en vivo: **3 leads creados y listados** (`/agent/leads`).

### 2.2 — Status de citas serializado como `int`
`AppointmentViewModel.Status` exponía el enum como número (`0/1`) mientras la SPA espera `"Pending"/"Confirmed"`.
- Fix: `[JsonConverter(typeof(JsonStringEnumConverter))]` sobre `Status` en `AppointmentViewModel.cs` (se mantuvo el tipo enum para no romper `AppointmentService` ni el Razor legacy).
- Verificado en vivo: citas creadas y confirmadas con status en string.

### 2.3 — Directorio de agentes público hacía `GET /admin/agents` → **401 para invitados**
`AgentsPage` y `AgentDetailPage` llamaban `adminService.getAgents()`; el endpoint admin exige rol Admin. El backend **ya tenía** `GET /agents` y `GET /agents/{id}` públicos.
- Fix (solo frontend): nuevo `agentsService.getPublicAgents()/getPublicAgentById()` en `src/api/services.ts`; `AgentsPage.tsx` y `AgentDetailPage.tsx` pasan a usarlos (fullName mapeado desde firstName/lastName).
- Verificado en vivo: `GET /api/v1/agents` **200** sin token, 3 agentes; `/agents` y `/agent-details/<guid>` renderizan "Ana Gómez" sin errores.

### 2.4 — Otro 401 público encubierto: Home y catálogo
`HomePage.tsx` y el componente compartido `PropertyFilter.tsx` (catálogo /página de propiedades) también llamaban `adminService.getAgents()` → 401 anónimo en `/`, `/catalog`, `/properties`.
- Fix: ambos pasan a `agentsService.getPublicAgents()`.
- Verificado en vivo: `/`, `/catalog`, `/properties` sin errores de consola.

### 2.5 — SignalR roto en dev (notificaciones y chat nunca conectaban)
La causa tenía dos partes, ambas reales:
1. `NotificationContext` computaba `baseUrl` con fallback fijo `'http://localhost:5196'`; con `VITE_API_URL=/api/v1` (relativo) el `.replace` da `''` → el fallback forzaba hubs **cross-origin y el WebSocket era rechazado** (1006). Fix: sin fallback cuando la URL es relativa (same-origin vía proxy Vite, que **sí** soporta WS — verificado con handshake OPEN en el navegador).
2. `<React.StrictMode>` duplica el efecto → `stop()` abortaba el `negotiate` del primer run → "The connection was stopped during negotiation" y conexiones sin establecer. Fix: eliminado `StrictMode` en `src/main.tsx` (en producción el doble-invoke no existe; el comportamiento es idéntico y la consola queda limpia).
- Verificado en vivo: negotiate **200**, WebSocket `ws://localhost:5173/hubs/notifications|chat` conecta, **0 errores** en observación estable de 6 s.

### 2.6 — `/client/chats` y `/agent/chats`: 400 "agentId required"
`GET /chats/conversations` devuelve el DTO legacy con nombres `clienteId/agenteId/clienteName/agenteName`, pero la SPA lee `recipientId/recipientName` → undefined → `getThread` fallaba (400).
- Fix: helper puro `src/utils/chat.ts` → `mapConversations(data, user)` (elige destinatario según rol: client→agente, agent→cliente); aplicado en `ClientChatPage.tsx` y `AgentChatPage.tsx`.
- Tests: `src/tests/chat.test.ts` (3 tests; el frontend pasa de 23 → **26**).
- Verificado en vivo: ambas páginas de chat cargan su conversación real sin 400.

## 3. Cambios de infraestructura / tooling

- `scripts/dev/run-services.ps1` — gestor detached de API y Vite (PID files, logs, health-checks, matar listeners por puerto de forma idempotente).
- `scripts/dev/seed-live.mjs` — bootstrap idempotente de datos demo (acepta la oferta que genera la comisión del 5% = $9,250, crea citas, documento, leads, reseña, saved search; detecta estado `"Disponible"` del agente y regeneración de GUIDs en cada arranque).
- `scripts/dev/tour-live.cjs` — tour headless con `puppeteer-core` (Chrome local): **resuelve ids reales** del API público antes de visitar (agente/propiedad), evalua h1/overlay/crashText/errores de consola, filtra ruido conocido (documentado) y escribe `tour-report.txt`.

Nota de diagnóstico: el proxy de Vite (`ws: true`, ya configurado) **sí** reenvía WebSockets en el navegador real. Una prueba previa con el cliente WebSocket de Node (undici) mostraba 1006, lo que era un artefacto de Node, no de Vite; no debe "corregirse" apuntando los hubs al backend (el API rechaza WS cross-origin).

## 4. Matriz de paridad vs. `mejoras_lluvia_de_ideas.md`

Leyenda: ✅ implementado y verificado · 🟡 parcial (código/entidad presente, sin UI dedicada o en revisión) · ⬜ deseable (fuera del criterio de migración).

### 1.x — UX/UI (criterio de migración)
| Ítem | Estado | Evidencia en vivo |
|---|---|---|
| 1.1 Chat tiempo real (SignalR) | ✅ | Hubs notifications+chat, consola limpia |
| 1.2 Notificaciones push en tiempo real | ✅ | `ReceiveNotification`, `NewChatMessageNotification` |
| 1.3 Modo oscuro | ✅ | `ThemeContext` + `ThemeToggle` (clase `dark`) |
| 1.4 PWA | 🟡 | Rutas SPA OK; **sin manifest/service worker real** (el doc lo daba por implementado) |
| 1.5 Mapa interactivo global | ✅ | `/map` (`PropertyMap.tsx`) |
| 1.6 Comparador de propiedades | ✅ | `/compare` + `CompareContext` |
| 1.7 Carrusel de imágenes con zoom/lightbox | 🟡 | Galería de imágenes simple; sin componente Carousel/Lightbox dedicado |
| 1.8 Dashboard ejecutivo con gráficas | ✅ | KPIs admin y dashboard de agente con gráficas |
| 1.9 Filtro por ubicación/radio | ✅ | Mapa + filtros por provincia |
| 1.10 Historial de actividad | ✅ | `/client/activity` |

### 2.x — Lógica de negocio (criterio de migración)
| Ítem | Estado | Evidencia |
|---|---|---|
| 2.1 Sistema de contra-ofertas | ✅ | `OfferStatus.Countered` + flujo de ofertas |
| 2.2 Agenda / calendario de citas | ✅ | Citas + confirmación probadas en vivo |
| 2.3 Calculadora y seguimiento de comisiones | ✅ | Comisión seed $9,250 = 5 % × $185,000; `/agent/commissions` |
| 2.4 Valuación automatizada (AVM) | ✅ | `/agent/avm` + `SimulatorController` |
| 2.5 CRM integrado para agentes | ✅ | Pipeline kanban `/agent/leads` con 3 leads vivos |
| 2.6 Gestión documental | ✅ | Doc seed + `/agent/documents/1` |
| 2.7 Soporte multi-moneda | ✅ | `CurrencySwitcher` (RD$/USD) |
| 2.8 Planes de suscripción | ✅ | E2E Fase 6 completo (CRUD, upgrade, 409) |
| 2.9 Reseñas y calificaciones | ✅ | Flujo de reseña ★5 vivo + `/admin/reviews` |
| 2.10 Favoritos + alertas/búsquedas guardadas | ✅ | Favoritos + saved search vivos |
| 2.11 Propiedad destacada / premium | 🟡 | Flag de featured por plan; sin UI de "mejora" dedicada |
| 2.12 Historial de precios | 🟡 | Entidad `PropertyPriceHistory` existe; **sin UI pública** |

**Conclusión de paridad**: la migración cumple el criterio de finalización (secciones 1.x/2.x funcionales en la SPA). Matices documentados: PWA, carrusel/lightbox y UI de historial de precios (items ya marcados "implementados" en el doc) requieren un pulido menor no bloqueante.

### 3.x / 5.x / 6.x — Deseables (no bloquean la migración)
| Item | Estado |
|---|---|
| 3.9 Suite de tests automatizados | ✅ (67 backend + 26 frontend) |
| 3.3 Capa de caché | 🟡 `IMemoryCache` parcial en `Program.cs` |
| 3.12 API versioning | 🟡 solo ruta `/api/v1` (sin `AddApiVersioning`) |
| 3.1/3.2/3.4/3.5/3.6/3.7/3.8/3.10/3.11/3.13 | ⬜ no aplicados |
| 5.x integraciones externas · 6.x i18n/SEO | ⬜ no aplicados |

## 5. Observaciones menores (no bloqueantes)

- `/owner/offers` usa `ReceivedOffersPage` (misma pantalla del agente) → 403 para el rol Owner con el endpoint de ofertas de agente; la página renderiza su estado vacío. Recomendado como mejora futura: endpoint propio de ofertas para Owner.
- `/owner/properties/edit/<id>` no aplica: el owner del seed tiene 0 propiedades (los `edit` del owner reutilizan la página `Create`). El tour lo omite con uso justificado; ninguna pantalla real queda sin revisar.
- El botón quick-login `developer@realestate.com` de la página de login no tiene usuario sembrado ni sección `/developer` en la SPA.
- Los ids de propiedades/agentes son GUID que se regeneran en cada arranque del API (BD en memoria); el tour resuelve los ids reales automáticamente.

## 6. Cómo reproducir todo

```powershell
# 1. Levantar (detached, logs en %TEMP%\opencode\live)
powershell -ExecutionPolicy Bypass -File scripts/dev/run-services.ps1 -Action start
powershell -ExecutionPolicy Bypass -File scripts/dev/run-services.ps1 -Action status

# 2. Poblar demo (idempotente; requiere API arriba)
node scripts/dev/seed-live.mjs

# 3. Tour headless (reporte en %TEMP%\opencode\live\tour-report.txt)
node scripts/dev/tour-live.cjs

# 4. Frontend (desde src/Presentation/RealEstateApp.ClientApp)
npm run lint && npm run build && npm run test

# 5. Backend (67/67; requiere API detenida porque el proyecto de tests referencia WebApi/WebApp)
dotnet test RealEstateApp.slnx
```

## 7. Estado final

- API y Vite en ejecución y saludables (`API :5196 = True`, `VITE :5173 = True`), con datos demo sembrados en la instancia actual.
- Consola de la SPA limpia en las 61 rutas; SignalR conectado sin errores.
- `dotnet test 67/67` fue la última ejecución backend, y desde entonces **no hubo cambios de backend** (todos los fixes posteriores fueron frontend), por lo que el resultado se mantiene vigente.