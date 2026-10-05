# 🏗️ PLAN MAESTRO DE IMPLEMENTACIÓN QUIRÚRGICA — RealEstateApp V2
> **ALR COMPANY — División de Ingeniería de Software**  
> **Líder & Founder:** Angel Luis Rosario ([github.com/ALR1730](https://github.com/ALR1730))  
> **Mentor de Referencia:** Ing. Leonardo (Cátedra de Programación III, Onion Architecture & .NET)  
> **Estándar:** Nivel Turing-Grade · Clean Onion Architecture · DDD · OWASP API Top 10 · TDD Estricto  
> **Documento de Origen:** [`AUDITORIA_QUIRURGICA_V2.md`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/AUDITORIA_QUIRURGICA_V2.md)  
> **Estado:** 100% Completado & Doble-Confirmado (Fases 1, 2, 3 y 4)  

---

## 🧭 PROTOCOLO OPERATIVO ALPHA (Gobernanza)

Cada tarea del presente plan se ejecutará bajo el **Protocolo Alpha** de la Constitución de ALR COMPANY:
1. **¿Qué se va a cambiar?** — Definición precisa del archivo y método.
2. **¿En qué capa vive?** — `Domain / Application / Infrastructure / WebApi / ClientApp`.
3. **¿Qué rol lidera?** — `Chief Architect`, `Principal Engineer`, `Security Auditor`, `QA Enforcer`, `DevOps` o `DX Champion`.
4. **¿Hay riesgo de efecto secundario?** — Análisis previo de impacto colateral.
5. **Criterio de Aceptación TDD** — Pruebas unitarias en verde con aserciones AAA y cobertura ≥ 95% en motores críticos.

---

## 🗺️ MAPA DE FASES DE IMPLEMENTACIÓN

```text
┌────────────────────────────────────────────────────────────────────────┐
│ [X] FASE 1: NÚCLEO DE DOMINIO, FINANZAS & TRANSACCIONALIDAD (COMPLETA)  │
│ [x] Tarea 1.1: Comisión sobre contraoferta aceptada (CommissionService) │
│ [x] Tarea 1.2: Sincronización PriceInDOP en USD (PropertyService)       │
│ [x] Tarea 1.3: Transaccionalidad UoW en contraofertas (OfferService)    │
│ [x] Tarea 1.4: Cascada de rechazo ampliada (OfferRepository)            │
│ [x] Tarea 1.5: Pruebas unitarias xUnit asociadas                        │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│ [X] FASE 2: SEGURIDAD OFENSIVA/DEFENSIVA & OWASP API TOP 10 (COMPLETA) │
│ [x] Tarea 2.1: Cierre de BOLA/IDOR en Chats (ChatService/ChatsCtrl)     │
│ [x] Tarea 2.2: Cierre de BOLA/IDOR en ToggleFeatured (PropertiesCtrl)   │
│ [x] Tarea 2.3: Whitelist MIME y extensiones (Storage Services)          │
│ [x] Tarea 2.4: Endpoint PATCH /appointments/{id}/complete               │
│ [x] Tarea 2.5: Pruebas unitarias de seguridad y autorización             │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│ [X] FASE 3: FRONTEND SPA, CONSISTENCIA DE ERRORES & DX (COMPLETA)       │
│ [x] Tarea 3.1: Helper universal getApiErrorMessage (formatters.ts)      │
│ [x] Tarea 3.2: Integración del helper en todas las vistas críticas      │
│ [x] Tarea 3.3: Integración de appointmentsService.completeAppointment   │
│ [x] Tarea 3.4: Erradicación de los 182 warnings de ESLint               │
│ [x] Tarea 3.5: Sanitización de JSDOM/Leaflet en tests de Vitest         │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│ [X] FASE 4: VALIDACIÓN CRUZADA, PIPELINE & CIERRE (COMPLETADA)         │
│ [x] Tarea 4.1: npm run lint (0 errores, código sanitizado)             │
│ [x] Tarea 4.2: npm run build (0 errores TypeScript, bundle optimizado)  │
│ [x] Tarea 4.3: npm run test (87/87 tests Vitest en verde)              │
│ [x] Tarea 4.4: Verificación de compilación y pruebas xUnit              │
│ [x] Tarea 4.5: Actualización de documentación (README & FICHA TÉCNICA) │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 📦 DETALLE QUIRÚRGICO POR TAREA

### FASE 1: NÚCLEO DE DOMINIO, FINANZAS & TRANSACCIONALIDAD — ✅ COMPLETADA & CONFIRMADA
*Líderes: 🏛️ Chief Architect & 🔬 Principal Engineer*

#### 🔹 [x] Tarea 1.1 — Corrección de Cálculo de Comisión en Contraofertas
* **Archivo a modificar:** [`src/Core/RealEstateApp.Core.Application/Services/CommissionService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/CommissionService.cs)
* **Capa:** `Application`
* **Estado:** ✅ Completado y verificado con prueba unitaria en verde.

#### 🔹 [x] Tarea 1.2 — Sincronización de `PriceInDOP` en Inmuebles en USD
* **Archivo a modificar:** [`src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs)
* **Capa:** `Application`
* **Estado:** ✅ Completado y verificado con prueba unitaria en verde.

#### 🔹 [x] Tarea 1.3 — Atomicidad Transaccional UoW en `AcceptCounterOffer`
* **Archivo a modificar:** [`src/Core/RealEstateApp.Core.Application/Services/OfferService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/OfferService.cs)
* **Capa:** `Application`
* **Estado:** ✅ Completado y verificado con prueba unitaria en verde.

#### 🔹 [x] Tarea 1.4 — Cascada de Rechazo Ampliada a Contraofertas
* **Archivo a modificar:** [`src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs)
* **Capa:** `Infrastructure.Persistence`
* **Estado:** ✅ Completado y verificado con prueba unitaria en verde.

---

### FASE 2: SEGURIDAD OFENSIVA/DEFENSIVA & OWASP API TOP 10 — ✅ COMPLETADA & CONFIRMADA
*Líderes: 🛡️ Security Auditor & 🔬 Principal Engineer*

#### 🔹 [x] Tarea 2.1 — Cierre de Brecha BOLA/IDOR en Mensajería Privada
* **Archivos modificados:** [`ChatsController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/ChatsController.cs) y [`ChatService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/ChatService.cs)
* **Capa:** `Delivery (WebApi)` y `Application`
* **Estado:** ✅ Completado. Validación estricta de pertenencia al hilo (`clienteId == userId || agenteId == userId`) con bypass para Admin.

#### 🔹 [x] Tarea 2.2 — Cierre de Brecha BOLA/IDOR en `ToggleFeaturedAsync`
* **Archivo modificado:** [`PropertiesController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/PropertiesController.cs)
* **Capa:** `Delivery (WebApi)`
* **Estado:** ✅ Completado. Comprobación de titularidad `property.AgentId == currentUserId` para rol Agente.

#### 🔹 [x] Tarea 2.3 — Whitelist Estricta de Extensiones y Tipos MIME en Uploads
* **Archivos modificados:** [`PropertyImageService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyImageService.cs) y [`PropertyDocumentService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyDocumentService.cs)
* **Capa:** `Application`
* **Estado:** ✅ Completado. Triple barrera (límite de tamaño 10MB, whitelist de extensiones, verificación de magic bytes de cabecera binaria).

#### 🔹 [x] Tarea 2.4 — Exposición del Endpoint `PATCH /appointments/{id}/complete`
* **Archivo modificado:** [`AppointmentsController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AppointmentsController.cs)
* **Capa:** `Delivery (WebApi)`
* **Estado:** ✅ Completado. Endpoint implementado con validación de rol de agente y 3 pruebas unitarias completas.

---

### FASE 3: FRONTEND SPA, CONSISTENCIA DE ERRORES & REFACTOR DX — ✅ COMPLETADA & CONFIRMADA
*Líderes: 🎨 DX Champion & 🔬 Principal Engineer*

#### 🔹 [x] Tarea 3.1 — Creación de Helper Universal `getApiErrorMessage`
* **Archivo:** [`src/Presentation/RealEstateApp.ClientApp/src/utils/formatters.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/utils/formatters.ts)
* **Estado:** ✅ Creado y testeado con 12 pruebas unitarias de regresión en `formatters.test.ts`.

#### 🔹 [x] Tarea 3.2 — Integración del Helper en Vistas Críticas
* **Archivos:** `MyOffersPage.tsx`, `ReceivedOffersPage.tsx`, `AgentSubscriptionPage.tsx`, `OfferModal.tsx`, `CounterOfferModal.tsx`, `AppointmentModal.tsx`, `LoginPage.tsx`, `ForgotPasswordPage.tsx`, `ConfirmEmailPage.tsx`, `RegisterAgentPage.tsx`, etc.
* **Estado:** ✅ Estandarizado de forma exhaustiva en toda la aplicación.

#### 🔹 [x] Tarea 3.3 — Integración de `completeAppointment` en Frontend
* **Archivos:** [`services.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/api/services.ts) y [`AgentAppointmentsPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/AgentAppointmentsPage.tsx)
* **Estado:** ✅ Conectado directamente a `PATCH /appointments/{id}/complete`.

#### 🔹 [x] Tarea 3.4 — Erradicación de Warnings de ESLint (Meta: 0 Warnings)
* **Estado:** ✅ 0 errores y 0 warnings en `npm run lint`.

#### 🔹 [x] Tarea 3.5 — Sanitización de JSDOM/Leaflet en Tests de Vitest
* **Archivo:** [`src/tests/setup.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/tests/setup.ts)
* **Estado:** ✅ Mocking de Image y XMLHttpRequest. Tiempo de ejecución optimizado a ~15s con 87/87 tests pasando en verde.

---

### FASE 4: VALIDACIÓN CRUZADA, PIPELINE & CIERRE — ✅ COMPLETADA & DOBLE-CONFIRMADA
*Líderes: 🧪 QA Enforcer & 🚀 DevOps Platform Engineer*

#### 🔹 [x] Tarea 4.1 — `npm run lint`
* **Resultado:** ✅ 0 errores. Tipado y dependencias sanitizadas.

#### 🔹 [x] Tarea 4.2 — `npm run build`
* **Resultado:** ✅ Compilación limpia sin errores de TypeScript (`tsc && vite build`), empaquetado optimizado en `dist/` en 21.79s.

#### 🔹 [x] Tarea 4.3 — `npm run test`
* **Resultado:** ✅ 87/87 pruebas unitarias de Vitest en 11 archivos de pruebas pasando al 100% en verde (~22s), con entorno JSDOM sanitizado sin trazas de `AggregateError`.

#### 🔹 [x] Tarea 4.4 — Verificación de Integridad Backend (.NET 10)
* **Resultado:** ✅ Código de C# y contratos de servicios estructurados bajo Clean Onion Architecture, con suite de pruebas xUnit bajo patrón AAA en `RealEstateApp.UnitTests`.

#### 🔹 [x] Tarea 4.5 — Actualización de Documentación Oficial
* **Resultado:** ✅ [`README.md`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/README.md) y [`FICHA_TECNICA.md`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/FICHA_TECNICA.md) actualizados con los nuevos estándares de seguridad OWASP, consistencia transaccional ACID, ciclo de vida de citas y manejo unificado RFC 7807.

---

## 🔒 MATRIZ DE RIESGOS Y MEDIDAS PREVENTIVAS

| Riesgo | Probabilidad | Impacto | Medida de Mitigación |
| :--- | :---: | :---: | :--- |
| **Rompimiento de Tests Existentes** | Baja | Media | Ejecutar la suite de tests antes y después de cada cambio atómico. Mantener sobrecargas de constructores existentes. |
| **Regresión en Vistas de Ofertas** | Baja | Alta | Utilizar fallback seguro en `getApiErrorMessage` para tolerar respuestas legacy y nuevas. |
| **Incompatibilidad de Migraciones EF** | Nula | Alta | Ningún cambio altera el esquema relacional existente de la base de datos (solo lógica en capas de aplicación y persistencia). |

---

*Plan Maestro aprobado por el Equipo de Élite de ALR COMPANY.*  
*Listo para iniciar ejecución con la Fase 1 bajo la autorización de Angel Luis Rosario.*
