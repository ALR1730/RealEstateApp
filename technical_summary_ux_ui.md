# 🎨 Ficha Técnica y Plan de Mejoras: Encabezado 1 — Experiencia de Usuario (UX/UI)

Este documento detalla el diseño técnico, arquitectura, componentes, entidades y tecnologías necesarias para implementar **todas las 10 mejoras de Experiencia de Usuario (UX/UI)** descritas en el encabezado 1 de `mejoras_lluvia_de_ideas.md` para **RealEstateApp V3**.

---

## 🏗️ 1. Resumen Arquitectónico del Módulo UX/UI

La actualización UX/UI transformará la aplicación WebApp MVC de un portal tradicional por SSR/polling a una **Single-Page-Like Hybrid Experience** con comunicación en tiempo real (SignalR), componentes interactivos (Chart.js, Leaflet, Swiper, GLightbox), almacenamiento local/Offline (PWA + Service Worker) y personalización del usuario (Modo Oscuro, Comparador, Historial de Actividades).

```
RealEstateApp (Solution)
 ├── src
 │    ├── Core
 │    │    ├── RealEstateApp.Core.Domain
 │    │    │    └── Entities
 │    │    │         ├── [NUEVO] Notification.cs
 │    │    │         └── [NUEVO] UserActivity.cs
 │    │    └── RealEstateApp.Core.Application
 │    │         ├── Interfaces/Services
 │    │         │    ├── [NUEVO] INotificationService.cs
 │    │         │    └── [NUEVO] IUserActivityService.cs
 │    │         └── ViewModels/DTOs
 │    │              ├── NotificationViewModel.cs
 │    │              └── UserActivityViewModel.cs
 │    ├── Infrastructure
 │    │    └── RealEstateApp.Infrastructure.Persistence
 │    │         ├── ApplicationDbContext.cs (DbSet<Notification>, DbSet<UserActivity>)
 │    │         └── Repositories
 │    │              ├── [NUEVO] NotificationRepository.cs
 │    │              └── [NUEVO] UserActivityRepository.cs
 │    └── Presentation
 │         └── RealEstateApp.Presentation.WebApp
 │              ├── Hubs
 │              │    ├── [NUEVO] ChatHub.cs (Mensajería en tiempo real)
 │              │    └── [NUEVO] NotificationHub.cs (Notificaciones push)
 │              ├── Controllers
 │              │    ├── ChatsController.cs (Integración de Hub)
 │              │    ├── NotificationsController.cs (Endpoints Ajax)
 │              │    ├── PropertiesController.cs (Map, Compare, GeoLocation)
 │              │    ├── AdminController.cs (Dashboard con Chart.js)
 │              │    └── AgentController.cs (Dashboard con Chart.js)
 │              └── wwwroot
 │                   ├── js/
 │                   │    ├── chat-realtime.js (SignalR Chat Client)
 │                   │    ├── notifications-realtime.js (SignalR Toast Client)
 │                   │    ├── theme-toggle.js (Dark Mode persistent script)
 │                   │    ├── property-comparator.js (Floating comparison drawer)
 │                   │    ├── property-map.js (Leaflet + MarkerCluster)
 │                   │    ├── geolocation-filter.js (Geolocation API)
 │                   │    └── sw.js (PWA Service Worker)
 │                   ├── manifest.json (PWA Manifest)
 │                   └── offline.html (PWA Offline Fallback Page)
```

---

## 📋 2. Detalle Técnico por Mejora (1.1 a 1.10)

### 1.1 Chat en Tiempo Real con SignalR
- **Mecanismo**: ASP.NET Core SignalR con transportes WebSockets / Server-Sent Events.
- **Hub Backend**: `ChatHub : Hub` en `WebApp/Hubs/ChatHub.cs` con métodos:
  - `SendMessage(int chatId, string receiverId, string message)`
  - `SendTypingIndicator(int chatId, string receiverId, bool isTyping)`
  - `MarkAsRead(int chatId, int messageId)`
- **Cliente Frontend**: `wwwroot/js/chat-realtime.js` usando `@microsoft/signalr`.
- **Experiencia UX**: Burbujas de chat que aparecen sin recargar la página, indicador de "Escribiendo...", scroll automático suave y estado de mensaje leído.

### 1.2 Notificaciones Push en Tiempo Real
- **Entidad de Dominio**: `Notification.cs` (`Id`, `UserId`, `Title`, `Message`, `Type`, `IsRead`, `RedirectUrl`, `CreatedAt`).
- **Hub Backend**: `NotificationHub : Hub` mapeado a `/hubs/notification`.
- **Disparadores (Triggers)**:
  - Recepción de mensaje de chat (`ChatHub`).
  - Nueva oferta enviada/recibida (`OfferService`).
  - Aceptación/Rechazo de oferta (`OfferService`).
  - Activación de cuenta por Administrador (`UserService`).
- **Frontend UI**:
  - Toasts emergentes animados (Bootstrap / Custom Glassmorphism Toast).
  - Ícono de campana en la barra superior (topbar) con contador en tiempo real (badge).
  - Menú desplegable con historial reciente y botón "Marcar todas como leídas".

### 1.3 Modo Oscuro (Dark Mode)
- **CSS Architecture**: Sistema de variables CSS globales en `wwwroot/css/site.css` (`--bg-body`, `--bg-card`, `--text-primary`, `--text-secondary`, `--border-color`, `--glass-bg`, `--glass-border`).
- **JavaScript Persistence**: `wwwroot/js/theme-toggle.js` guardando el estado en `localStorage` (`theme=dark` / `theme=light`) y sincronizando con la preferencia del sistema operativo (`prefers-color-scheme`).
- **Control UI**: Botón de alternancia (Sol / Luna) integrado en la barra de navegación principal.

### 1.4 Progressive Web App (PWA)
- **Manifest**: `wwwroot/manifest.json` definiendo `name`, `short_name`, `theme_color` (`#0f172a`), `background_color`, `display: standalone`, e íconos responsivos (192x192, 512x512).
- **Service Worker**: `wwwroot/sw.js` administrando la caché de assets estáticos (CSS, JS, fuentes, imágenes de UI) y ofreciendo la vista `wwwroot/offline.html` en caso de pérdida de conexión a internet.
- **Instalación UX**: Banner flotante o botón de "Instalar App" en el header/footer cuando el evento `beforeinstallprompt` es capturado.

### 1.5 Vista de Mapa Interactivo Global
- **Librería JS**: Leaflet.js + Leaflet.markercluster.
- **Endpoint Data**: `GET /Properties/GetMapData` retornando JSON de propiedades con sus coordenadas geográficas (`Latitude`, `Longitude`), fotos, precio, título y enlace a detalle.
- **Vista**: `Views/Properties/Map.cshtml` ofreciendo mapa en pantalla completa, marcadores agrupados (clusters), popup interactivo con tarjeta desplegable de propiedad y filtros dinámicos (Venta/Alquiler, Tipo de Propiedad, Rango de Precio).

### 1.6 Comparador de Propiedades
- **Gestión de Estado**: `wwwroot/js/property-comparator.js` manteniendo una lista de IDs de propiedades en `localStorage` (máximo 4 propiedades).
- **Drawer Flotante UI**: Barra inferior persistente que muestra miniaturas de las propiedades seleccionadas, contador y botón "Comparar Ahora".
- **Vista Dedicada**: `Views/Properties/Compare.cshtml` desplegando una tabla comparativa lado a lado:
  - Precio y Monto de Separación
  - Tamaño en m²
  - Habitaciones, Baños, Parqueos
  - Tipo de Propiedad y Venta
  - Badges de Mejoras (aire acondicionado, piscina, etc.)
  - Mini mapa comparativo

### 1.7 Carrusel de Imágenes con Zoom y Lightbox
- **Librerías**: Swiper.js (carrusel táctil para dispositivos móviles) + GLightbox (pantalla completa con zoom, touch-swipe e información de pie de foto).
- **Vista**: `Views/Properties/Details.cshtml` actualizado con:
  - Slider principal de imágenes.
  - Tira de miniaturas (thumbnails) para navegación rápida.
  - Ícono de expansión para abrir la galería Lightbox a pantalla completa con soporte de Zoom.

### 1.8 Dashboard Ejecutivo con Gráficas Interactivas
- **Librería JS**: Chart.js / ApexCharts.
- **Endpoints de Datos**: `GET /Admin/GetDashboardChartsData` y `GET /Agent/GetDashboardChartsData`.
- **Gráficos Integrados**:
  - **Gráfico de Dona**: Distribución de propiedades por estado (Disponible, Vendida).
  - **Gráfico de Barras**: Cantidad de propiedades por tipo (Apartamento, Casa, Villa, Penthouse, etc.).
  - **Línea de Tendencia**: Evolución mensual de ofertas realizadas vs aceptadas.
  - **Tarjetas de KPI**: Sparklines con % de variación respecto al mes anterior.

### 1.9 Filtro por Ubicación / Radio Geográfico
- **Captura Geográfica**: Geolocation API del navegador (`navigator.geolocation.getCurrentPosition`).
- **Cálculo Backend**: Fórmula de Haversine integrada en `PropertyService` / LINQ para calcular distancias en kilómetros.
- **Control UI**: Slider en la barra de búsqueda del catálogo (`Views/Properties/Index.cshtml`): "Buscar a menos de X km de mi posición actual".

### 1.10 Historial de Actividad del Usuario
- **Entidad de Dominio**: `UserActivity.cs` (`Id`, `UserId`, `Action`, `Description`, `Icon`, `TargetUrl`, `CreatedAt`).
- **Servicio Backend**: `UserActivityService.cs` para registrar eventos clave (Oferta enviada, Mensaje enviado, Favorito agregado, Cita solicitada).
- **Componente Visual**: Timeline interactivo en la vista de Perfil / Dashboard (`Views/Account/Profile.cshtml` o `Views/User/Activity.cshtml`) con insignias de estado, horas relativas ("Hace 5 min") y enlaces a las entidades relacionadas.

---

## 🧪 3. Plan de Verificación

1. **Pruebas de Tiempo Real (SignalR)**: Abrir dos navegadores (uno como Cliente, otro como Agente), enviar un mensaje en el chat y verificar que aparezca instantáneamente en ambos sin recargar.
2. **Pruebas de Notificaciones**: Realizar una oferta con el Cliente y verificar que el Agente reciba el Toast y la campana se incremente.
3. **Pruebas de Tema y PWA**: Probar el alternador de modo oscuro, recargar la página y verificar persistencia. Verificar auditoría de PWA en Chrome Lighthouse.
4. **Pruebas de Mapa y Comparador**: Seleccionar propiedades para comparar y verificar renderizado en mapa Leaflet y vista de comparación lado a lado.
5. **Pruebas de Dashboard**: Verificar que Chart.js renderice los gráficos con datos reales de la base de datos SQL Server.
