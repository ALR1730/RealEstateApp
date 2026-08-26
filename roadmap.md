# 🗺️ Roadmap de Evolución — RealEstateApp V2

Este documento traza las fases de desarrollo completadas y las líneas maestras de evolución futura para la plataforma inmobiliaria **RealEstateApp**.

---

## 🏁 Fase 1: Arquitectura Núcleo & Migración a React SPA [COMPLETADO ✅]

* [x] **Backend RESTful (.NET 10 ASP.NET Core Web API)**:
  * [x] Implementación de Arquitectura Onion (Domain, Application, Persistence, Shared, WebApi).
  * [x] Autenticación y Autorización por Tokens JWT Bearer para todos los roles (`Admin`, `Agent`, `Client`, `Developer`).
  * [x] Eliminación de bloqueos cruzados en endpoints de autenticación.
  * [x] Endpoints REST completos para Propiedades, Ofertas, Citas, Favoritos, Búsquedas Guardadas, Chats y Catálogos.
  * [x] Hubs en tiempo real con SignalR (`/hubs/chat` y `/hubs/notifications`).
  * [x] Configuración de políticas de CORS restrictivas y soporte de subida de archivos estáticos.
  * [x] Documentación interactiva con Swagger OpenAPI 3.0.

* [x] **Frontend SPA de Alta Gama en React**:
  * [x] Inicialización con **Vite + React + TypeScript**.
  * [x] Sistema de diseño con **Tailwind CSS**, efectos *Glassmorphism*, tipografías *Inter* y *Outfit*, e iconografía *Lucide React*.
  * [x] **Módulo Público:** Landing Page dinámica con buscador rápido, Catálogo de Propiedades con filtrado reactivo multicriterio, Detalle de Propiedad con galería lightbox (hasta 15 fotos), directorio de agentes y simulador hipotecario independiente.
  * [x] **Simulador Hipotecario Dominicano:** Cálculos precisos en Pesos Dominicanos (RD$) bajo el Sistema Francés de Amortización con tabla desglosada y soporte de impresión.
  * [x] **Portal del Cliente:** Favoritos, historial de ofertas, citas agendadas, búsquedas guardadas con alertas, chat directo y perfil.
  * [x] **Portal del Agente:** Dashboard analítico, publicación/edición de inmuebles con fotos y amenidades, bandeja de ofertas con regla atómica de cierre, gestión de visitas y chat en vivo.
  * [x] **Portal del Administrador:** Cuadro de mando ejecutivo de KPIs, mantenimiento y reasignación de agentes, gestión de usuarios admins/developers, y CRUD de catálogos núcleo.

* [x] **Suite Integral de Pruebas Unitarias**:
  * [x] Backend (.NET xUnit + Moq + FluentAssertions): 25 pruebas unitarias 100% en verde.
  * [x] Frontend (Vitest + Testing Library): Pruebas de amortización francesa en RD$, formateadores de moneda y componentes clave.

* [x] **Actualización Exhaustiva de Documentación**:
  * [x] `README.md` actualizado con diagramas de arquitectura desacoplada, credenciales de demostración y guías de inicio.
  * [x] `installation and configuration.md` con instrucciones paso a paso para Backend y Frontend.

---

## 🚀 Fase 2: Expansión Móvil & Servicios Inteligentes [PRÓXIMAMENTE 🔮]

* [ ] **Aplicación Móvil Nativa (React Native / Flutter)**: Consumo directo de la Web API JWT existente para iOS y Android.
* [ ] **Inteligencia Artificial para Tasación Predictiva**: Estimación del valor de mercado por metro cuadrado según histórico transaccional del sector.
* [ ] **Firma Digital de Contratos de Reserva**: Generación automatizada de contrato preliminar de compraventa con firma electrónica.
* [ ] **Integración de Pasarela de Pagos (Carnet / Azul)**: Cobro directo de depósitos de reserva y membresías de agentes.

---

© 2026 **RealEstateApp** — Ecosistema Inmobiliario de Alto Rendimiento.
