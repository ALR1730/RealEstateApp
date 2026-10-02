# 🏗️ PLAN MAESTRO DE IMPLEMENTACIÓN QUIRÚRGICA — RealEstateApp V2
> **ALR COMPANY — División de Ingeniería de Software**  
> **Líder & Founder:** Angel Luis Rosario ([github.com/ALR1730](https://github.com/ALR1730))  
> **Mentor de Referencia:** Ing. Leonardo (Cátedra de Programación III, Onion Architecture & .NET)  
> **Estándar:** Nivel Turing-Grade · Clean Onion Architecture · DDD · OWASP API Top 10 · TDD Estricto  
> **Documento de Origen:** [`AUDITORIA_QUIRURGICA_V2.md`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/AUDITORIA_QUIRURGICA_V2.md)  
> **Estado:** Listo para Ejecución Secuencial  

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
│ FASE 1: NÚCLEO DE DOMINIO, FINANZAS & TRANSACCIONALIDAD               │
│ - Tarea 1.1: Comisión sobre contraoferta aceptada (CommissionService) │
│ - Tarea 1.2: Sincronización PriceInDOP en USD (PropertyService)       │
│ - Tarea 1.3: Transaccionalidad UoW en contraofertas (OfferService)    │
│ - Tarea 1.4: Cascada de rechazo ampliada (OfferRepository)            │
│ - Tarea 1.5: Pruebas unitarias xUnit asociadas                        │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│ FASE 2: SEGURIDAD OFENSIVA/DEFENSIVA & OWASP API TOP 10                │
│ - Tarea 2.1: Cierre de BOLA/IDOR en Chats (ChatService/ChatsCtrl)     │
│ - Tarea 2.2: Cierre de BOLA/IDOR en ToggleFeatured (PropertiesCtrl)   │
│ - Tarea 2.3: Whitelist MIME y extensiones (Storage Services)          │
│ - Tarea 2.4: Endpoint PATCH /appointments/{id}/complete               │
│ - Tarea 2.5: Pruebas unitarias de seguridad y autorización             │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│ FASE 3: FRONTEND SPA, CONSISTENCIA DE ERRORES & REFACTOR DX            │
│ - Tarea 3.1: Helper universal getApiErrorMessage (formatters.ts)      │
│ - Tarea 3.2: Integración del helper en todas las vistas críticas      │
│ - Tarea 3.3: Integración de appointmentsService.completeAppointment   │
│ - Tarea 3.4: Erradicación de los 182 warnings de ESLint               │
│ - Tarea 3.5: Sanitización de JSDOM/Leaflet en tests de Vitest         │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │
┌───────────────────────────────────▼────────────────────────────────────┐
│ FASE 4: VALIDACIÓN CRUZADA, VERIFICACIÓN DE PIPELINE & CIERRE          │
│ - Tarea 4.1: npm run lint (0 errores, 0 warnings)                     │
│ - Tarea 4.2: npm run build (0 errores TypeScript, bundle optimizado)  │
│ - Tarea 4.3: npm run test (100% tests Vitest en verde sin warnings)   │
│ - Tarea 4.4: dotnet build & dotnet test (100% xUnit en verde)         │
│ - Tarea 4.5: Informe final de perfeccionamiento entregable            │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 📦 DETALLE QUIRÚRGICO POR TAREA

### FASE 1: NÚCLEO DE DOMINIO, FINANZAS & TRANSACCIONALIDAD
*Líderes: 🏛️ Chief Architect & 🔬 Principal Engineer*

#### 🔹 Tarea 1.1 — Corrección de Cálculo de Comisión en Contraofertas
* **Archivo a modificar:** [`src/Core/RealEstateApp.Core.Application/Services/CommissionService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/CommissionService.cs)
* **Capa:** `Application`
* **Cambio exacto:**
  Evaluar si `offer.CounterOfferAmount.HasValue && offer.CounterOfferAmount.Value > 0`. De ser así, usar dicho monto como base de cálculo del `SalePrice` y del `Amount` de la comisión; de lo contrario, usar `offer.MontoOfertado`.
* **Riesgo colateral:** Ninguno. Mejora la precisión financiera sin alterar el contrato público del DTO/ViewModel.
* **Prueba unitaria:** [`CommissionServiceTests.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/tests/RealEstateApp.UnitTests/Services/CommissionServiceTests.cs):
  `CreateForAcceptedOfferAsync_DebeCalcularComisionSobreMontoContraoferta_CuandoOfertaTieneContraofertaAceptada()`

#### 🔹 Tarea 1.2 — Sincronización de `PriceInDOP` en Inmuebles en USD
* **Archivo a modificar:** [`src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs)
* **Capa:** `Application`
* **Cambio exacto:**
  En los métodos `Add` y `Update`, obtener la tasa de cambio vigente a través de `_currencyService.GetExchangeRateAsync()` e invocar el método de dominio `property.ChangePrice(vm.Price, vm.Currency, exchangeRate)`. Esto garantiza que `PriceInDOP` se persista siempre sincronizado en la base de datos.
* **Riesgo colateral:** Ninguno. Habilita de inmediato a `PropertyValuationService` (AVM) para indexar propiedades en USD dentro de los comparables.
* **Prueba unitaria:** [`PropertyServiceTests.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/tests/RealEstateApp.UnitTests/Services/PropertyServiceTests.cs):
  `Add_DebeCalcularPriceInDOPCorrectamente_CuandoMonedaEsUSD()` y `Update_DebeActualizarPriceInDOP_CuandoPrecioOCurrencyCambian()`

#### 🔹 Tarea 1.3 — Atomicidad Transaccional UoW en `AcceptCounterOffer`
* **Archivo a modificar:** [`src/Core/RealEstateApp.Core.Application/Services/OfferService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/OfferService.cs)
* **Capa:** `Application`
* **Cambio exacto:**
  Envolver las invocaciones de `AcceptOfferTransactionAsync`, `CreateForAcceptedOfferAsync` y `LogActivityAsync` dentro de `_unitOfWork.ExecuteTransactionAsync` (idéntico a como se hace en `AcceptOffer`), asegurando rollback completo si falla la comisión o el registro de auditoría.
* **Riesgo colateral:** Ninguno. Consolida la consistencia ACID.
* **Prueba unitaria:** [`OfferServiceTests.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/tests/RealEstateApp.UnitTests/Services/OfferServiceTests.cs):
  `AcceptCounterOffer_DebeEjecutarseEnTransaccionUoW_CuandoSeAceptaContraoferta()`

#### 🔹 Tarea 1.4 — Cascada de Rechazo Ampliada a Contraofertas
* **Archivo a modificar:** [`src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs)
* **Capa:** `Infrastructure.Persistence`
* **Cambio exacto:**
  En `AcceptOfferCoreAsync`, modificar el predicado de búsqueda de ofertas competidoras para incluir aquellas cuyo estado sea `OfferStatus.Pending` o `OfferStatus.CounterOffered`:
  ```csharp
  .Where(o => o.PropertyId == offer.PropertyId && o.Id != offerId && 
             (o.Status == OfferStatus.Pending || o.Status == OfferStatus.CounterOffered))
  ```
* **Riesgo colateral:** Ninguno. Evita que queden contraofertas huérfanas en propiedades vendidas.
* **Prueba unitaria:** [`OfferRepositoryTests.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/tests/RealEstateApp.UnitTests/Services/OfferServiceStateTests.cs):
  `AcceptOfferTransactionAsync_DebeRechazarOfertasYContraofertasPendientes_AlCerrarVenta()`

---

### FASE 2: SEGURIDAD OFENSIVA/DEFENSIVA & OWASP API TOP 10
*Líderes: 🛡️ Security Auditor & 🔬 Principal Engineer*

#### 🔹 Tarea 2.1 — Cierre de Brecha BOLA/IDOR en Mensajería Privada
* **Archivos a modificar:**  
  - [`src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/ChatsController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/ChatsController.cs)
  - [`src/Core/RealEstateApp.Core.Application/Services/ChatService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/ChatService.cs)
* **Capa:** `Delivery (WebApi)` y `Application`
* **Cambio exacto:**
  En `ChatService.GetChatThread`, validar que `currentUserId == clienteId || currentUserId == agenteId`, permitiendo bypass solo si el usuario posee el rol `Admin`. De lo contrario, lanzar `ValidationException("No tiene autorización para acceder a esta conversación privada.")`.
* **Prueba unitaria:** [`ChatServiceTests.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/tests/RealEstateApp.UnitTests/Services/ChatServiceTests.cs):
  `GetChatThread_DebeLanzarExcepcion_CuandoUsuarioNoPerteneceALaConversacion()`

#### 🔹 Tarea 2.2 — Cierre de Brecha BOLA/IDOR en `ToggleFeaturedAsync`
* **Archivo a modificar:** [`src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/PropertiesController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/PropertiesController.cs)
* **Capa:** `Delivery (WebApi)`
* **Cambio exacto:**
  Antes de invocar `_propertyService.ToggleFeaturedAsync`, obtener la propiedad vía `_propertyService.GetByIdViewModel(id)`. Si el usuario tiene rol `Agent` y `property.AgentId != currentUserId`, responder `Forbid()`.
* **Prueba unitaria:** [`PropertiesControllerTests.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/tests/RealEstateApp.UnitTests/Controllers/PropertiesControllerTests.cs):
  `ToggleFeaturedAsync_DebeRetornarForbid_CuandoAgenteIntentaModificarInmuebleAjeno()`

#### 🔹 Tarea 2.3 — Whitelist Estricta de Extensiones y Tipos MIME en Uploads
* **Archivos a modificar:**  
  - [`src/Core/RealEstateApp.Core.Application/Services/PropertyImageService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyImageService.cs)
  - [`src/Core/RealEstateApp.Core.Application/Services/PropertyDocumentService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyDocumentService.cs)
* **Capa:** `Application`
* **Cambio exacto:**
  Definir listas blancas inmutables:
  - Imágenes: `.jpg`, `.jpeg`, `.png`, `.webp` (MIMEs: `image/jpeg`, `image/png`, `image/webp`).
  - Documentos: `.pdf`, `.docx`, `.png`, `.jpg`, `.jpeg` (MIMEs correspondientes).
  Rechazar cualquier archivo fuera de este conjunto con `ValidationException`.
* **Prueba unitaria:** `PropertyDocumentServiceTests.cs`:
  `UploadAsync_DebeLanzarValidationException_CuandoExtensionNoEstaEnListaBlanca()`

#### 🔹 Tarea 2.4 — Exposición del Endpoint `PATCH /appointments/{id}/complete`
* **Archivo a modificar:** [`src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AppointmentsController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AppointmentsController.cs)
* **Capa:** `Delivery (WebApi)`
* **Cambio exacto:**
  Agregar el endpoint formal:
  ```csharp
  [Authorize(Roles = "Agent")]
  [HttpPatch("{id:int}/complete")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> CompleteAppointmentAsync(int id, [FromBody] string? notes)
  {
      var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(agentId)) return Unauthorized();

      await _appointmentService.CompleteAppointmentAsync(id, agentId, notes);
      return Ok(new { success = true, message = "Cita completada exitosamente." });
  }
  ```

---

### FASE 3: FRONTEND SPA, CONSISTENCIA DE ERRORES & REFACTOR DX
*Líderes: 🎨 DX Champion & 🔬 Principal Engineer*

#### 🔹 Tarea 3.1 — Creación de Helper Universal `getApiErrorMessage`
* **Archivo a modificar:** [`src/Presentation/RealEstateApp.ClientApp/src/utils/formatters.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/utils/formatters.ts)
* **Capa:** `ClientApp (Utils)`
* **Cambio exacto:**
  Crear y exportar `getApiErrorMessage(error: unknown, fallback?: string): string`. La función inspeccionará con type-guards seguros:
  1. `data?.error`
  2. `data?.message`
  3. `data?.title`
  4. Mensajes de validación en arrays `data?.errors`
  5. Fallback por defecto.
* **Prueba unitaria:** [`formatters.test.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/tests/formatters.test.ts).

#### 🔹 Tarea 3.2 — Integración del Helper en Vistas Críticas
* **Archivos a modificar:**
  - [`MyOffersPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/client/MyOffersPage.tsx)
  - [`ReceivedOffersPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/ReceivedOffersPage.tsx)
  - [`AgentSubscriptionPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/AgentSubscriptionPage.tsx)
  - Modales: [`OfferModal.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/components/offers/OfferModal.tsx), [`CounterOfferModal.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/components/offers/CounterOfferModal.tsx), [`AppointmentModal.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/components/appointments/AppointmentModal.tsx)
* **Cambio exacto:** Reemplazar el acceso manual `err.response?.data?.message` por `getApiErrorMessage(err, "...")`.

#### 🔹 Tarea 3.3 — Integración de `completeAppointment` en Frontend
* **Archivos a modificar:**  
  - [`src/Presentation/RealEstateApp.ClientApp/src/api/services.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/api/services.ts)
  - [`AgentAppointmentsPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/AgentAppointmentsPage.tsx)
* **Cambio exacto:**  
  Agregar `completeAppointment: (id: number, notes?: string)` en `appointmentsService` apuntando a `PATCH /appointments/{id}/complete`. Actualizar `handleComplete` en `AgentAppointmentsPage.tsx` para invocarlo directamente.

#### 🔹 Tarea 3.4 — Erradicación de Warnings de ESLint (Meta: 0 Warnings)
* **Archivos a intervenir:**  
  - Reemplazo sistemático de `(err: any)` por `(err: unknown)`.
  - Eliminación de imports huérfanos de `lucide-react` en componentes y páginas de admin.
  - Envolvimiento de `loadAll` con `useCallback` en [`AgentDocumentsPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/AgentDocumentsPage.tsx).
  - Limpieza de dependencias en `useEffect` de [`NotificationContext.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/context/NotificationContext.tsx).

#### 🔹 Tarea 3.5 — Sanitización de JSDOM/Leaflet en Tests de Vitest
* **Archivo a modificar:** [`src/Presentation/RealEstateApp.ClientApp/src/tests/setup.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/tests/setup.ts)
* **Cambio exacto:**  
  Mockear globalmente el constructor de `Image` y las llamadas XMLHttpRequest no interceptadas de mapas para que la salida de `npm run test` sea 100% limpia sin trazas de `AggregateError`.

---

### FASE 4: VALIDACIÓN CRUZADA, PIPELINE & CIERRE
*Líderes: 🧪 QA Enforcer & 🚀 DevOps Platform Engineer*

#### 🔹 Tareas de Verificación Continua:
1. `npm run lint` — Confirmación de 0 errores y 0 warnings.
2. `npm run build` — Confirmación de compilación exitosa y generación limpia de bundles.
3. `npm run test` — 79+ pruebas de Vitest pasando en verde sin ruido en consola.
4. `dotnet test RealEstateApp.slnx` — 100% de pruebas unitarias xUnit pasando en verde.
5. Actualización de [`README.md`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/README.md) y [`FICHA_TECNICA.md`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/FICHA_TECNICA.md) reflejando los nuevos estándares de seguridad y consistencia financiera.

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
