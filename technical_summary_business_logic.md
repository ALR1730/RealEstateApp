# 💼 Resumen Técnico: Mejoras de Lógica de Negocio

Este documento detalla la arquitectura técnica, modelos de datos, flujos de trabajo, controladores y servicios necesarios para implementar todas las mejoras planteadas en el **Encabezado 2: Mejoras de Lógica de Negocio** del proyecto **RealEstateApp V3**.

---

## 📋 Lista de Mejoras (Sección 2)

### 2.1 Sistema de Contra-Ofertas [✅ Implementado]
- **Estado**: ✅ Implementado y funcional al 100%.
- **Descripción**: Permite a los agentes enviar contra-propuestas de precio y términos cuando una oferta no cumple sus expectativas. El cliente puede revisar la contra-oferta, aceptarla (ejecutando la venta y el rechazo en cascada de ofertas competidoras) o rechazarla.
- **Cambios en Dominio**:
  - `OfferStatus.cs`: Añadido el valor `CounterOffered` ("Contra-Ofertada").
  - `Offer.cs`: Propiedades `decimal? CounterOfferAmount`, `string? CounterOfferMessage`, `DateTime? CounterOfferDate`.
- **Cambios en Persistencia**:
  - `ApplicationDbContext.cs`: Mapeo de columna con precisión `decimal(18,2)` para `CounterOfferAmount` y `HasMaxLength(1000)` para `CounterOfferMessage`.
- **Capa de Aplicación y Servicios**:
  - `CounterOfferViewModel.cs`: Modelo de formulario para la emisión de contra-ofertas del agente.
  - `OfferViewModel.cs`: Extendidos campos de contra-oferta y propiedad calculada `StatusFormatted`.
  - `IOfferService.cs` / `OfferService.cs`:
    - `CounterOffer(int offerId, decimal counterAmount, string? counterMessage, string agentUserId)`: Actualiza estado a `CounterOffered` y registra la actividad en `UserActivityService`.
    - `AcceptCounterOffer(int offerId, string clientUserId)`: Aceptación por parte del cliente que invoca `AcceptOfferTransactionAsync` (marca propiedad como `Sold` y rechaza otras ofertas en cascada).
    - `RejectCounterOffer(int offerId, string clientUserId)`: Rechazo y cierre de negociación por parte del cliente.
- **Controladores y Vistas (UI)**:
  - `AgentController.cs`: Endpoint `[HttpPost] CounterOffer(CounterOfferViewModel vm)`.
  - `OffersController.cs`: Endpoints `[HttpPost] AcceptCounterOffer(int offerId)` y `[HttpPost] RejectCounterOffer(int offerId)`.
  - `Views/Agent/Offers.cshtml`: Badge `Contra-Ofertada` y **Modal Interactivo "Contra-Ofertar"** para ingresar nuevo monto y notas.
  - `Views/Offers/MyOffers.cshtml`: Banner de propuesta destacada con los botones *"Aceptar Contra-Oferta"* y *"Rechazar"*.

### 2.2 Agenda de Visitas / Calendario de Citas [✅ Implementado]
- **Estado**: ✅ Implementado y funcional al 100%.
- **Descripción**: Módulo completo de agendamiento donde los clientes solicitan visitas presenciales/virtuales desde la ficha del inmueble (`Home/Details.cshtml`), los agentes aprueban, rechazan o completan las citas, y ambos visualizan su agenda en un calendario interactivo alimentado por **FullCalendar.js**.
- **Entidad de Dominio**:
  - `PropertyAppointment.cs`: `Id`, `PropertyId`, `ClienteId`, `AgentId`, `AppointmentDate`, `Status` (`AppointmentStatus`: `Pending`, `Confirmed`, `Cancelled`, `Completed`), `Notes`, `AgentNotes`.
- **Persistencia**:
  - `ApplicationDbContext.cs`: Configuración de la tabla `PropertyAppointments` con `DbSet<PropertyAppointment>` y eliminación en cascada vinculada a `Property`.
  - `IAppointmentRepository.cs` / `AppointmentRepository.cs`: Métodos `GetByPropertyIdAsync`, `GetByClienteIdAsync`, `GetByAgentIdAsync` con incluye de navegación.
- **Capa de Aplicación y Servicios**:
  - `AppointmentViewModel.cs` & `SaveAppointmentViewModel.cs`: Modelos de datos para renderizado y solicitudes.
  - `IAppointmentService.cs` / `AppointmentService.cs`:
    - `RequestAppointmentAsync(SaveAppointmentViewModel vm, string clienteId)`: Validación de fecha futura, creación de cita en estado `Pending` y auditoría en `UserActivityService`.
    - `ConfirmAppointmentAsync(int appointmentId, string agentId, string? agentNotes)`: Confirmación por el agente y notificación en timeline del cliente.
    - `CancelAppointmentAsync(int appointmentId, string userId, string? reason)`: Cancelación por el cliente o agente.
    - `CompleteAppointmentAsync(int appointmentId, string agentId, string? agentNotes)`: Cierre de cita efectuada con éxito.
- **Controladores y Vistas (UI)**:
  - `AppointmentsController.cs`: Endpoints `[HttpPost] RequestAppointment`, `[HttpGet] MyAppointments`, `[HttpGet] AgentCalendar`, `[HttpGet] GetCalendarEvents` (JSON feed para FullCalendar), `[HttpPost] Confirm`, `[HttpPost] Cancel`, `[HttpPost] Complete`.
  - `Views/Appointments/MyAppointments.cshtml`: Panel del cliente con cards y badges de estado.
  - `Views/Appointments/AgentCalendar.cshtml`: Agenda del agente con **FullCalendar.js**, vista interactiva por mes/semana/día, modal de detalles y panel lateral de gestión rápida.
  - `Views/Shared/_Layout.cshtml`: Enlaces de navegación *"Mis Citas de Visita"* (Cliente) y *"Agenda de Citas"* (Agente).

### 2.3 Calculadora y Seguimiento de Comisiones
- **Descripción**: Monitoreo de comisiones acumuladas, pagadas y pendientes por venta/alquiler para agentes.
- **Cambios en Dominio**:
  - `CommissionRate` configurable por propiedad o por agente (ej. 5% de la venta).
  - Campo `CommissionAmount` en `Property` o registro en `CommissionLedger`.
- **Servicios**:
  - `IAgentCommissionService.GetAgentCommissionSummaryAsync(string agentId)`.
- **UI**:
  - Sección en el Dashboard de Agente con KPI Cards (Comisiones Totales, Pendientes de Cobro, Tasa Promedio) y exportación a PDF/Excel.

### 2.4 Valuación Automatizada de Propiedades (AVM)
- **Descripción**: Motor de sugerencia de precios de mercado estimado basado en algoritmo de inmuebles comparables en la misma zona (barrio/ciudad, tipo, m² y habitaciones).
- **Servicio**:
  - `IPropertyValuationService.CalculateEstimatedValueAsync(int propertyTypeId, int rooms, int bathrooms, double sizeInMeters, double lat, double lng)`.
- **UI**:
  - Widget interactivo "Precio Sugerido de Mercado" en el formulario de creación/edición de inmuebles del Agente y en el simulador de compradores.

### 2.5 CRM Integrado / Sales Pipeline para Agentes
- **Descripción**: Embudo visual tipo Kanban (Interesado → Cita → Oferta → Negociación → Cierre) para gestionar prospectos.
- **Entidades de Dominio**:
  - `AgentLead`: `Id`, `AgentId`, `ClienteId` o `ContactName`, `Email`, `Phone`, `Stage`, `PropertyId`, `Notes`, `NextFollowUpDate`.
- **UI**:
  - Tablero Kanban interactivo con drag-and-drop en la vista de Agente (`Agent/Pipeline`).

### 2.6 Gestión Documental y Almacenamiento Seguro
- **Descripción**: Carga y vinculación de archivos PDF/imágenes legales (Título de propiedad, Carta de pre-aprobación bancaria, Contratos).
- **Dominio**:
  - `PropertyDocument`: `Id`, `PropertyId`, `DocumentType`, `FileUrl`, `UploadedBy`, `UploadedAt`.
- **Solución al Bug de Carta de Pre-aprobación**:
  - Modificar `OffersController.cs` para almacenar en disco (`/uploads/offers/`) el archivo de la carta de pre-aprobación y guardar su ruta en la entidad `Offer`.

### 2.7 Soporte Multi-Moneda (USD + RD$)
- **Descripción**: Conversión dinámica de precios entre Pesos Dominicanos (RD$) y Dólares Estadounidenses (USD).
- **Servicio**:
  - `ICurrencyService`: Servicio de conversión con tasa fija configurable o consulta a API externa de tipo de cambio.
- **UI**:
  - Selector de moneda global en el Header o conmutador de divisa en las tarjetas de propiedades.

### 2.8 Planes de Suscripción para Agentes
- **Descripción**: Sistema de membresías (Gratuito, Pro, Elite) que limita la cantidad de publicaciones activas y ofrece badges verificados.
- **Dominio**:
  - `AgentSubscription`: `AgentId`, `PlanType`, `MaxProperties`, `IsVerified`, `ExpirationDate`.
- **Middlewares / Validaciones**:
  - `AgentController.CreateProperty` valida si el agente ha alcanzado el límite de su plan antes de permitir crear otra propiedad.

### 2.9 Sistema de Reseñas y Calificaciones a Agentes
- **Descripción**: Valoraciones de 1 a 5 estrellas con comentario entregadas por clientes tras completar transacciones.
- **Dominio**:
  - `AgentReview`: `Id`, `AgentId`, `ClienteId`, `Rating` (1-5), `Comment`, `CreatedAt`.
- **UI**:
  - Sección de Reseñas y estrellas promedio en el Perfil de Agente y en el detalle de la propiedad.

### 2.10 Favoritos Inteligentes con Alertas de Precio
- **Descripción**: Enviar notificaciones automáticas y correo cuando una propiedad en la lista de favoritos sufre una rebaja de precio o cambia de estado.
- **Servicios**:
  - `IFavoriteAlertService.CheckAndNotifyPriceDropAsync(int propertyId, decimal oldPrice, decimal newPrice)`.

### 2.11 Propiedades Destacadas (Featured Listings)
- **Descripción**: Posicionamiento prioritario en la parte superior del catálogo con distintivo visual "DESTACADO / FEATURED".
- **Dominio**:
  - `Property.IsFeatured`, `Property.FeaturedUntil`.

### 2.12 Historial de Precios de Propiedades
- **Descripción**: Registro de variaciones históricas del valor de una propiedad y renderizado de gráfica de evolución temporal.
- **Dominio**:
  - `PropertyPriceHistory`: `Id`, `PropertyId`, `Price`, `ChangedAt`, `ChangedBy`.
- **UI**:
  - Gráfico de línea con Chart.js en la vista `Home/Details.cshtml`.
