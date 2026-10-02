# 🔬 AUDITORÍA QUIRÚRGICA INTEGRAL — RealEstateApp V2
> **ALR COMPANY — División de Ingeniería de Software**  
> **Autor & Founder:** Angel Luis Rosario ([github.com/ALR1730](https://github.com/ALR1730))  
> **Mentor de Referencia:** Ing. Leonardo (Cátedra de Programación III, Onion Architecture & .NET)  
> **Comité de Revisión:** Equipo de Ingeniería de Élite (6 Roles Activos)  
> **Estándar:** Turing-Grade · Clean Onion Architecture · DDD · OWASP Top 10 API  
> **Fecha:** Octubre 2026 · **Estado:** Hallazgos & Plan Maestro de Perfeccionamiento  

---

## 📑 ÍNDICE GENERAL
1. [Resumen Ejecutivo de la Auditoría](#1-resumen-ejecutivo-de-la-auditoría)
2. [Vector I: Dominio, Finanzas & Reglas de Negocio (Crítico)](#2-vector-i-dominio-finanzas--reglas-de-negocio)
3. [Vector II: Seguridad Ofensiva & Defensiva (OWASP API Top 10)](#3-vector-ii-seguridad-ofensiva--defensiva-owasp)
4. [Vector III: Integridad Transaccional & Base de Datos](#4-vector-iii-integridad-transaccional--base-de-datos)
5. [Vector IV: Frontend SPA & Experiencia de Usuario (React 18 / Vite)](#5-vector-iv-frontend-spa--experiencia-de-usuario)
6. [Vector V: Testing, Cobertura & Calidad de Código](#6-vector-v-testing-cobertura--calidad-de-código)
7. [Matriz de Priorización & Plan de Acción Quirúrgico](#7-matriz-de-priorización--plan-de-acción-quirúrgico)

---

## 1. RESUMEN EJECUTIVO DE LA AUDITORÍA

El presente documento constituye la auditoría técnica más rigurosa, profunda y exhaustiva realizada sobre **RealEstateApp** hasta la fecha. El sistema evidencia un nivel de ingeniería sobresaliente: arquitectura Onion estrictamente desacoplada, 26 entidades de dominio con lógica rica, frontend reactivo en React 18 con Vite y Tailwind CSS, y una suite automatizada de pruebas sólida (79/79 en Vitest, 165+ en xUnit).

Sin embargo, para consolidar este proyecto como la **joya insignia de portafolio profesional** de Angel Luis Rosario ante comités técnicos, arquitectos de software senior e inversores internacionales, se han identificado **8 hallazgos quirúrgicos** (4 de lógica de negocio/financiera, 2 de seguridad OWASP y 2 de consistencia frontend/API) que requieren resolución inmediata.

---

## 2. VECTOR I: DOMINIO, FINANZAS & REGLAS DE NEGOCIO

### 🔴 Hallazgo 1.1 — Cálculo Erróneo de Comisión en Cierres por Contraoferta
* **Archivo:** [`src/Core/RealEstateApp.Core.Application/Services/CommissionService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/CommissionService.cs#L63-L66)
* **Severidad:** **CRÍTICA (Impacto Financiero Directo)**
* **Módulo:** `CommissionService.CreateForAcceptedOfferAsync`

#### Diagnóstico del Defecto:
Cuando un cliente acepta una **contraoferta** emitida por el agente, el precio final de venta pactado queda registrado en `offer.CounterOfferAmount`. Sin embargo, `CommissionService` utiliza ciegamente `offer.MontoOfertado`:
```csharp
// Código Vulnerable en CommissionService.cs:
var commission = new Commission
{
    AgentId = property.AgentId,
    PropertyId = property.Id,
    OfferId = offer.Id,
    SalePrice = offer.MontoOfertado, // <-- DEFECTO: Ignora la contraoferta aceptada
    Rate = rate,
    Amount = Math.Round(offer.MontoOfertado * rate / 100m, 2),
    Status = CommissionStatus.Pending
};
```
* **Impacto:** Si un cliente ofertó RD$ 5,000,000, el agente contraofertó RD$ 5,500,000 y el cliente aceptó, la comisión se calcula sobre los RD$ 5,000,000 originales, subpagando la comisión del agente y generando discrepancias en los reportes de ingresos de la inmobiliaria.

#### Solución Quirúrgica:
```csharp
decimal finalSalePrice = (offer.CounterOfferAmount.HasValue && offer.CounterOfferAmount.Value > 0)
    ? offer.CounterOfferAmount.Value
    : offer.MontoOfertado;

var commission = new Commission
{
    AgentId = property.AgentId,
    PropertyId = property.Id,
    OfferId = offer.Id,
    SalePrice = finalSalePrice,
    Rate = rate,
    Amount = Math.Round(finalSalePrice * rate / 100m, 2),
    Status = CommissionStatus.Pending
};
```

---

### 🔴 Hallazgo 1.2 — Desincronización de `PriceInDOP` en Inmuebles en USD (Falla en Tasador AVM)
* **Archivo:** [`src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyService.cs#L172-L186) y [`PropertyValuationService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyValuationService.cs#L45)
* **Severidad:** **ALTA (Afecta Integridad del Algoritmo AVM y Catálogo)**
* **Módulo:** `PropertyService.Add` y `PropertyService.Update`

#### Diagnóstico del Defecto:
La entidad de dominio `Property` posee el método `ChangePrice(decimal newPrice, string? currency, decimal? customUsdRate)` para recalcular automáticamente `PriceInDOP`. No obstante, tanto en `PropertyService.Add` como en `PropertyService.Update`, los valores se asignan directamente desde el ViewModel:
```csharp
// En PropertyService.Update:
property.Price = vm.Price;
property.Currency = newCurrency;
// DEFECTO: PriceInDOP nunca se actualiza si la propiedad está en USD!
```
* **Impacto:**
  1. Si un inmueble se registra en USD (ej. US$ 300,000), `PriceInDOP` queda en `0`.
  2. En el servicio de tasación automática ([`PropertyValuationService.cs:45`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyValuationService.cs#L45)):
     ```csharp
     var pricePerSqmList = comparables
         .Where(p => p.SizeInMeters > 0 && p.PriceInDOP > 0) // <-- Se descartan propiedades en USD!
     ```
     Las propiedades en dólares son excluidas silenciosamente del cálculo de avalúos automáticos por metro cuadrado.

#### Solución Quirúrgica:
En `PropertyService.Add` y `PropertyService.Update`, invocar `property.ChangePrice(vm.Price, vm.Currency)` utilizando la tasa de cambio provista por `ICurrencyService`.

---

## 3. VECTOR II: SEGURIDAD OFENSIVA & DEFENSIVA (OWASP)

### 🔴 Hallazgo 2.1 — BOLA / IDOR en Acceso a Mensajería Privada (OWASP API1:2023)
* **Archivo:** [`src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/ChatsController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/ChatsController.cs#L41-L48) y [`ChatService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/ChatService.cs#L31-L44)
* **Severidad:** **CRÍTICA (Violación de Privacidad & Fuga de Datos)**
* **Módulo:** `ChatsController.GetThreadAsync`

#### Diagnóstico del Defecto:
El endpoint `GET /api/v1/chats/thread?clientId={cId}&agentId={aId}&propertyId={pId}` requiere autenticación (`[Authorize]`), pero **no valida** que el usuario autenticado (`userId`) sea el cliente (`clientId`), el agente (`agentId`) o un administrador (`Admin`).
```csharp
// Código Vulnerable en ChatService.cs:
public async Task<List<ChatViewModel>> GetChatThread(
    string clienteId, string agenteId, int? propertyId, string currentUserId)
{
    // DEFECTO: Devuelve los mensajes sin validar autorización sobre la conversación!
    var messages = await _chatRepository.GetChatThreadAsync(clienteId, agenteId, propertyId);
    var viewModels = _mapper.Map<List<ChatViewModel>>(messages);
    foreach (var vm in viewModels) { vm.IsMine = vm.SenderId == currentUserId; }
    return viewModels;
}
```
* **Impacto:** Cualquier usuario autenticado en la plataforma (incluso un cliente ajeno o agente competidor) puede leer el historial de chat completo entre cualquier cliente y agente con solo enviar sus IDs en la URL.

#### Solución Quirúrgica:
```csharp
if (currentUserId != clienteId && currentUserId != agenteId)
{
    throw new ValidationException("No tiene autorización para acceder a esta conversación privada.");
}
```

---

### 🔴 Hallazgo 2.2 — BOLA / IDOR en Alternancia de Inmuebles Destacados (OWASP API1:2023)
* **Archivo:** [`src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/PropertiesController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/PropertiesController.cs#L202-L217)
* **Severidad:** **ALTA**
* **Módulo:** `PropertiesController.ToggleFeaturedAsync`

#### Diagnóstico del Defecto:
A diferencia de los endpoints `PUT` y `DELETE` de propiedades (que validan estrictamente `if (User.IsInRole("Agent") && existing.AgentId != currentUserId) return Forbid();`), el endpoint `POST /api/v1/properties/{id}/toggle-featured` carece de verificación de pertenencia:
```csharp
[Authorize(Roles = "Agent,Admin")]
[HttpPost("{id:int}/toggle-featured")]
public async Task<IActionResult> ToggleFeaturedAsync(int id, [FromQuery] int durationDays = 30)
{
    // DEFECTO: Cualquier agente puede destacar o quitar el destacado a propiedades ajenas!
    await _propertyService.ToggleFeaturedAsync(id, durationDays);
    return Ok(...);
}
```

#### Solución Quirúrgica:
Verificar que si el invocador es `Agent`, la propiedad le pertenezca antes de delegar al servicio:
```csharp
var existing = await _propertyService.GetByIdViewModel(id);
if (existing == null) return NotFound();

var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
if (User.IsInRole("Agent") && existing.AgentId != currentUserId)
{
    return Forbid();
}
```

---

### ⚠️ Hallazgo 2.3 — Carga Desprotegida de Archivos / Whitelist MIME (CWE-434)
* **Archivos:** [`PropertyImageService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyImageService.cs) y [`PropertyDocumentService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/PropertyDocumentService.cs)
* **Severidad:** **MEDIA (Defensa en Profundidad)**

#### Diagnóstico del Defecto:
No se valida la extensión ni el Content-Type contra una lista blanca permitida. Se debe garantizar que únicamente extensiones seguras (`.jpg`, `.jpeg`, `.png`, `.webp` para imágenes; `.pdf`, `.docx`, `.png`, `.jpg` para documentos) puedan ser guardadas en `wwwroot/uploads`.

---

## 4. VECTOR III: INTEGRIDAD TRANSACCIONAL & BASE DE DATOS

### 🔴 Hallazgo 3.1 — Ausencia de `IUnitOfWork.ExecuteTransactionAsync` en Aceptación de Contraofertas
* **Archivo:** [`src/Core/RealEstateApp.Core.Application/Services/OfferService.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Core/RealEstateApp.Core.Application/Services/OfferService.cs#L148-L157)
* **Severidad:** **ALTA (Riesgo de Inconsistencia de Estado)**
* **Módulo:** `OfferService.AcceptCounterOffer`

#### Diagnóstico del Defecto:
En `OfferService.AcceptOffer`, las operaciones de cierre de propiedad y creación de comisión se ejecutan bajo `_unitOfWork.ExecuteTransactionAsync`. Sin embargo, en `AcceptCounterOffer`:
```csharp
// Código actual en AcceptCounterOffer:
offer.Status = OfferStatus.Pending;
await _offerRepository.UpdateAsync(offer);
await _offerRepository.AcceptOfferTransactionAsync(offerId);
await _commissionService.CreateForAcceptedOfferAsync(offerId); // <-- Fuera de la transacción de UoW!
```
* **Impacto:** Si `_commissionService.CreateForAcceptedOfferAsync` arroja una excepción, la oferta ya fue marcada como aceptada y la propiedad como Vendida sin registrar la comisión, dejando la base de datos en un estado huérfano irrecuperable.

#### Solución Quirúrgica:
Envolver ambas llamadas dentro de `_unitOfWork.ExecuteTransactionAsync` igual que en `AcceptOffer`.

---

### ⚠️ Hallazgo 3.2 — Cascada de Rechazo Incompleta (Contraofertas Huérfanas)
* **Archivo:** [`src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Infrastructure/RealEstateApp.Infrastructure.Persistence/Repositories/OfferRepository.cs#L98-L105)
* **Severidad:** **MEDIA**

#### Diagnóstico del Defecto:
Al vender una propiedad, el método `AcceptOfferCoreAsync` solo busca ofertas competidoras con `Status == OfferStatus.Pending`. Las ofertas en estado `OfferStatus.CounterOffered` no son rechazadas, quedando como contraofertas "activas" en un inmueble que ya está vendido. Debe incluir `o.Status == OfferStatus.Pending || o.Status == OfferStatus.CounterOffered`.

---

## 5. VECTOR IV: FRONTEND SPA & EXPERIENCIA DE USUARIO

### ⚠️ Hallazgo 4.1 — Discrepancia en Formato de Errores de API
* **Archivos:** [`MyOffersPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/client/MyOffersPage.tsx#L69), [`ReceivedOffersPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/ReceivedOffersPage.tsx#L49), [`AgentSubscriptionPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/AgentSubscriptionPage.tsx#L109)
* **Severidad:** **MEDIA (Experiencia de Usuario / DX)**

#### Diagnóstico del Defecto:
Varios controladores devuelven `{ hasError: true, error: "Mensaje explicativo" }`. Las vistas frontend leen `err.response?.data?.message`. Al ser `undefined`, la interfaz cae siempre en el texto genérico de reserva (*"Error al aceptar oferta"* o *"Error al enviar reseña"*), ocultando al usuario la causa real reportada por el servidor.

#### Solución Quirúrgica:
Implementar en [`src/utils/formatters.ts`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/utils/formatters.ts):
```typescript
export function getApiErrorMessage(error: unknown, fallback: string = 'Ha ocurrido un error inesperado.'): string {
  if (!error || typeof error !== 'object') return fallback;
  const res = (error as { response?: { data?: { error?: string; message?: string; title?: string } } })?.response?.data;
  return res?.error || res?.message || res?.title || fallback;
}
```

---

### ⚠️ Hallazgo 4.2 — Falta de Endpoint `Complete` en Controlador de Citas
* **Archivos:** [`AppointmentsController.cs`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.Presentation.WebApi/Controllers/v1/AppointmentsController.cs) y [`AgentAppointmentsPage.tsx`](file:///c:/Users/DELL/Desktop/wordspace/RealEstateApp/src/Presentation/RealEstateApp.ClientApp/src/pages/agent/AgentAppointmentsPage.tsx#L113)
* **Severidad:** **BAJA-MEDIA**

`AppointmentService` implementa `CompleteAppointmentAsync`, pero `AppointmentsController` no expone `[HttpPatch("{id:int}/complete")]`. La vista de citas del agente recurría a llamar `confirmAppointment` con notas en lugar de transicionar la cita al estado formal `Completed`.

---

## 6. VECTOR V: TESTING, COBERTURA & CALIDAD DE CÓDIGO

| Área | Estado Actual | Meta Turing-Grade |
| :--- | :---: | :---: |
| **Frontend Tests** | 79 pruebas pasando en 11 suites | 100% pasando sin advertencias de red en consola de JSDOM |
| **Frontend Linter** | 0 errores / 182 warnings | 0 errores / 0 warnings (limpieza total de `any`) |
| **Backend Tests** | 165+ pruebas unitarias | Incorporación de tests específicos para contraofertas y AVM |
| **Cobertura de Negocio** | Cierre de ofertas 100% testeado | Cascada de contraofertas y comisiones en contraoferta cubiertas |

---

## 7. MATRIZ DE PRIORIZACIÓN & PLAN DE ACCIÓN QUIRÚRGICO

```text
[FASE 1: CORE DOMAIN & TRANSACCIONALIDAD]
  ├── Corregir cálculo de comisión para contraofertas (CommissionService.cs)
  ├── Sincronizar PriceInDOP en Add y Update de propiedades (PropertyService.cs)
  ├── Envolver AcceptCounterOffer en Unit of Work Transaction (OfferService.cs)
  └── Extender cascada de rechazo a contraofertas (OfferRepository.cs)

[FASE 2: SEGURIDAD & AUTORIZACIÓN]
  ├── Proteger ChatsController.GetThreadAsync contra espionaje BOLA (ChatService.cs)
  ├── Validar propiedad del agente en PropertiesController.ToggleFeaturedAsync
  ├── Exponer endpoint PATCH /appointments/{id}/complete (AppointmentsController.cs)
  └── Implementar validación de extensiones y tipos MIME permitidos

[FASE 3: FRONTEND UNIFICATION & LINTING]
  ├── Centralizar parseo de errores con getApiErrorMessage() en ClientApp
  ├── Corregir llamadas de Appointments en AgentAppointmentsPage.tsx
  ├── Depurar los 182 warnings de ESLint y eliminar archivos huérfanos de Vite
  └── Mockear Image en setup.ts para limpiar la consola de pruebas

[FASE 4: VERIFICACIÓN AUTOMATIZADA]
  ├── Escribir pruebas unitarias xUnit para los nuevos casos de contraoferta y cálculo de comisión
  └── Ejecución limpia de `npm run test`, `npm run lint` y `npm run build`
```

---
*Documento de Ingeniería generado por el Equipo de Élite de ALR COMPANY.*  
*Aprobado para ejecución bajo la supervisión de Angel Luis Rosario.*
