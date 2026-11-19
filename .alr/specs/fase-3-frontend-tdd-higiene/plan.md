# Contrato ALPHA — Fase 3: Frontend Estricto, TDD Social (`>= 95%`) e Higiene DX

> **Fase:** 3 de 3 (`fase-3-frontend-tdd-higiene`)  
> **Prerrequisito:** Fase 2 cerrada y verificada con `alr-zeta`  
> **Migraciones de BD:** **Ninguna** (`0` cambios de esquema)  
> **Roles líderes:** QA & TDD (`alr-qa-tdd`) + Principal Engineer (`GAMMA`) + DX Champion

---

## 1. Sub-bloques y Archivos Afectados

### 3.1 Frontend Grado Turing (`ClientApp`)
| Área | Archivo(s) | Cambio |
| :--- | :--- | :--- |
| **TypeScript Estricto** | `src/Presentation/RealEstateApp.ClientApp/tsconfig.json` | Activar `"strict": true`, `"noUnusedLocals": true`, `"noUnusedParameters": true`. |
| **Tipado & Modularización API** | `src/api/services.ts` → dividir en `src/api/{auth,properties,offers,appointments,admin,subscriptions,aiSearch}.api.ts` | Erradicar los **79 `any` explícitos** y los **115 warnings ESLint** (meta: `0 errors, 0 warnings`). |
| **Observabilidad Cliente** | `src/utils/logger.ts` *(Nuevo)* + reemplazo de los `137` `console.*` en `src/` | Logger centralizado que suprime logs debug/info en `import.meta.env.PROD`. |
| **SignalR Cleanup Seguro** | `src/context/NotificationContext.tsx` | Añadir flag `let cancelled = false` en `useEffect` para prevenir conexiones huérfanas bajo `React.StrictMode` y usar `crypto.randomUUID()` en lugar de `Math.random()`. |
| **i18n & SRP UI** | `src/context/LanguageContext.tsx` (`2,278` líneas) → `src/i18n/es.json` + `src/i18n/en.json`; páginas `CreateEditPropertyPage.tsx` (`640`), `ManageSubscriptionsPage.tsx` (`525`), `CreateOwnerPropertyPage.tsx` (`506`), `PropertyDetailPage.tsx` (`501`), `BuyAbilityPage.tsx` (`500`) | Extraer diccionarios a JSON lazy; dividir formularios y vistas grandes en subcomponentes cohesivos (`<= 250` líneas por componente) y aplicar `React.lazy` a rutas pesadas (`recharts`, `@fullcalendar`). |

### 3.2 TDD Social Anti-Mockitis (`>= 95%` en Core) & Pirámide `70 / 20 / 10`
| Suite | Archivo(s) | Cambio |
| :--- | :--- | :--- |
| **Tests de Dominio & Core Financiero** | `tests/RealEstateApp.UnitTests/Domain/*Tests.cs`, `MortgageAmortizationCalculatorTests.cs`, `FinancingServiceTests.cs`, `BuyAbilityServiceTests.cs`, `CommissionServiceTests.cs` | Alcanzar **`>= 95%` de cobertura de línea/rama** en pricing, hipoteca, comisiones, ofertas y agenda (con casos límite nulos, cero, negativos, desbordamientos y concurrencia). |
| **Erradicación de Mockitis (`architecture_standards.md` §3)** | `OfferServiceTests.cs`, `PropertyServiceTests.cs`, `SubscriptionServiceTests.cs`, `PropertyImageServiceTests.cs`, `CommissionServiceTests.cs`, `AppointmentServiceTests.cs` | Convertir pruebas centradas en `repositoryMock.Verify(...)` (actualmente `101` `.Verify()` vs `2` `InMemoryDb`) a pruebas sociales basadas en estado sobre `ApplicationDbContext` (`UseInMemoryDatabase` / SQLite InMemory) verificando el estado real persistido. Reservar `Mock<T>` únicamente para I/O externo (`IEmailService`, `IFileStorageService`, `IPaymentService`). |
| **Cobertura de Controladores & Authz (`18` faltantes)** | `tests/RealEstateApp.UnitTests/Controllers/*Tests.cs` | Cubrir los `18` controladores sin pruebas, priorizando matriz de autorización/BOLA/RFC 7807 en `OffersController`, `PropertiesController`, `DocumentsController`, `VerificationsController`, `SubscriptionsController`, `LeadsController`, `CommissionsController`. |
| **Frontend + E2E (`10%`)** | `src/Presentation/RealEstateApp.ClientApp/src/tests/*` + `e2e/*.spec.ts` (Playwright) | Añadir pruebas de componentes/páginas críticas, silenciar stderr intencional en `ErrorBoundary.test.tsx`, y crear suite E2E Playwright para flujos core (Login, Catálogo/Filtros, Oferta + Cierre, Simulador Hipotecario, Agendamiento de Cita). |

### 3.3 Higiene de Repositorio & Sincronización DX
| Área | Elemento | Cambio |
| :--- | :--- | :--- |
| **Eliminación de Residuos** | `context7/`, `sequential-thinking/`, `shadcn/`, `src/Presentation/RealEstateApp.Presentation.WebApp/` | Eliminar proyectos C# mock de MCP y carpeta legacy huérfana (⚠️ confirmar con Angel Luis Rosario antes de borrar). |
| **Archivo Documental** | `34` archivos `.md` sueltos en la raíz | Mover informes/resúmenes históricos a `docs/archive/`, dejando en raíz únicamente `README.md`, `AGENTS.md` y `FICHA_TECNICA.md`. |
| **Eliminación de ViewModels Legacy** | `src/Core/RealEstateApp.Core.Application/ViewModels/` (`45` archivos) | Migrar a `DTOs/{Modulo}/*Request` y `*Response` y eliminar la carpeta `ViewModels/` de `Core.Application`. |
| **DX (`AGENTS.md` & `README.md`)** | `README.md`, `AGENTS.md` | Actualizar arquitectura real (.NET 10, PostgreSQL/SQL Server, User Secrets, `Result<T>`, `ProblemDetails`, `LanguageContext` bilingüe, Refresh Tokens, comandos verificables en `<= 5` pasos). |

---

## 2. Criterios de Aceptación (`GIVEN / WHEN / THEN`)

1. **TypeScript & ESLint Cero Deuda:**
   - `GIVEN` `src/Presentation/RealEstateApp.ClientApp`
   - `WHEN` se ejecutan `npm run lint`, `npm run build` (`tsc && vite build`) y `npm run test`
   - `THEN` finalizan con exit code `0`, **`0` errores y `0` warnings** ESLint, `"strict": true` activo, `0` usos de `any` y `0` llamadas `console.*` en código fuente de producción.
2. **Cobertura & Anti-Mockitis Backend:**
   - `GIVEN` la suite `tests/RealEstateApp.UnitTests`
   - `WHEN` se ejecuta `dotnet test RealEstateApp.slnx --collect:"XPlat Code Coverage"`
   - `THEN` el `100%` de tests pasa, todos los servicios transaccionales validan estado real en BD en memoria, ningún controlador v1 carece de suite de pruebas, y la cobertura en dominio/pricing/ofertas/agenda/comisiones es **`>= 95%`**.
3. **Suite E2E Playwright:**
   - `GIVEN` WebAPI y ClientApp levantados en entorno de pruebas E2E
   - `WHEN` corre `npx playwright test`
   - `THEN` pasan los flujos end-to-end críticos (autenticación, búsqueda, oferta + cierre atómico, simulador, cita).
4. **Cero ViewModels en Application & Raíz Limpia:**
   - `GIVEN` el repositorio tras la limpieza
   - `WHEN` se listan archivos en raíz y en `src/Core/RealEstateApp.Core.Application/`
   - `THEN` la raíz solo contiene `README.md`, `AGENTS.md`, `FICHA_TECNICA.md` (los históricos están en `docs/archive/`), no existen `context7/`, `sequential-thinking/`, `shadcn/` ni `WebApp/`, y no quedan archivos en `Core.Application/ViewModels/`.

---

## 3. Riesgos y Confirmaciones Requeridas

- Activar `"strict": true` en `tsconfig.json` revelará errores de nulabilidad en múltiples páginas; debe abordarse módulo por módulo (primero `types/` y `api/`, luego `context/`, luego `components/` y `pages/`) manteniendo el build verde al cerrar cada módulo.
- Eliminar `context7/`, `sequential-thinking/`, `shadcn/` y `RealEstateApp.Presentation.WebApp/` requiere confirmación explícita de **Angel Luis Rosario** antes de ejecutar el borrado.
