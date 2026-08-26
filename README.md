# 🏢 RealEstateApp — Plataforma Integral de Gestión e Intermediación Inmobiliaria 🏠💼

[![Frontend](https://img.shields.io/badge/Frontend-React%2018%20%2F%20Vite%20%2F%20Tailwind-cyan.svg)](https://react.dev/)
[![Backend](https://img.shields.io/badge/Backend-.NET%2010%20%2F%20ASP.NET%20Core%20Web%20API-purple.svg)](https://dotnet.microsoft.com/)
[![Moneda](https://img.shields.io/badge/Moneda-RD%24%20Pesos%20Dominicanos-blue.svg)]()
[![Arquitectura](https://img.shields.io/badge/Arquitectura-Onion%20%2B%20SPA%20Desacoplada-orange.svg)]()
[![Pruebas](https://img.shields.io/badge/Pruebas-xUnit%20%2B%20Vitest%20%28100%25%20Verdes%29-brightgreen.svg)]()

---

## 📌 Resumen Ejecutivo para la Alta Dirección

**RealEstateApp** es un ecosistema tecnológico transaccional de última generación diseñado específicamente para **firmas inmobiliarias, desarrolladoras de proyectos y corredores de bienes raíces** en República Dominicana.

La plataforma cuenta con una arquitectura moderna desacoplada compuesta por un **Frontend SPA en React** (Vite + Tailwind CSS + Lucide Icons + SignalR) y un **Backend robusto en .NET 10 ASP.NET Core Web API**, transformando el catálogo pasivo tradicional en una **plataforma interactiva de aceleración de ventas**.

---

## 💡 Ventajas de Negocio y Reglas Críticas

### 🎯 1. Regla Atómica de Cierre de Ventas en Cascada
- **Cierre Garantizado:** Al aceptar una propuesta económica, el sistema cambia instantáneamente el estado del inmueble a **"Vendida"**, rechazando automáticamente en cascada las demás ofertas pendientes y bloqueando nuevas solicitudes.
- **Acceso Exclusivo al Comprador:** El comprador que cierra la negociación conserva acceso exclusivo a la propiedad para continuar la comunicación con su agente.

### 💰 2. Simulador Hipotecario Profesional en Pesos Dominicanos (RD$)
- **Sistema de Amortización Francés:** Cálculo interactivo de cuota mensual fija en RD$ con desglose de inicial, capital financiado e intereses acumulados.
- **Visualización y Descarga:** Generación de tabla amortizada año por año con soporte de impresión y exportación en PDF.

### 💬 3. Chat Bidireccional en Tiempo Real (SignalR / WebSockets)
- Cada propiedad cuenta con su propio hilo de mensajería directa entre el cliente interesado y el agente asignado, con notificaciones instantáneas de mensajes nuevos.

### 📊 4. Cuadro de Mando Ejecutivo (Dashboard KPIs)
- Métricas consolidadas en tiempo real: Inmuebles disponibles, reservados y vendidos; fuerza de ventas activa/inactiva y clientes registrados.
- Herramienta directiva para reasignación masiva de cartera entre agentes con un solo clic.

---

## 🏗️ Arquitectura Técnica de la Solución

```text
└── RealEstateApp
    ├── src/Presentation/RealEstateApp.ClientApp      # Frontend SPA en React (Vite, Tailwind, TypeScript, SignalR)
    ├── src/Presentation/RealEstateApp.Presentation.WebApi # Backend RESTful .NET 10 con JWT Bearer y SignalR Hubs
    ├── src/Core/RealEstateApp.Core.Domain            # Entidades puras de dominio (Propiedades, Ofertas, Citas, Chats)
    ├── src/Core/RealEstateApp.Core.Application       # Servicios de negocio, DTOs, simulador financiero y contratos
    ├── src/Infrastructure/RealEstateApp.Infrastructure.Persistence # EF Core 9/10 con SQL Server y semillas automáticas
    ├── src/Infrastructure/RealEstateApp.Infrastructure.Shared     # Servicios de infraestructura compartida (Cultura es-DO)
    └── tests/RealEstateApp.UnitTests                 # Suite integral de pruebas unitarias xUnit, Moq y FluentAssertions
```

---

## 👥 Experiencia por Perfil de Usuario

### 🛒 Para el Cliente / Inversionista (Comprador)
* **Búsqueda Avanzada y Filtros:** Búsqueda en tiempo real por tipo de inmueble, tipo de venta, precio en RD$, habitaciones y amenidades.
* **Módulo de Ofertas y Favoritos:** Historial transparente con estado de ofertas (*Pendiente, Aceptada, Rechazada*) y panel de favoritos.
* **Agendamiento de Visitas:** Selección de fecha y turno horario para visitas presenciales con confirmación del agente.

### 👔 Para el Agente Inmobiliario (Corredor)
* **Publicación de Portafolio:** Carga de hasta 15 fotografías, video tours, recorridos virtuales 360° y amenidades.
* **Bandeja de Ofertas y Negociación:** Aceptación con ejecución de la regla atómica de cierre y rechazo de ofertas competidoras.
* **Calendario de Visitas:** Confirmación y cancelación de citas agendadas por clientes.

### 🏢 Para la Dirección Inmobiliaria (Administrador)
* **Control de Corredores:** Activación, inactivación, reasignación de cartera y eliminación segura de agentes.
* **Mantenimiento de Catálogos Núcleo:** CRUD de tipos de propiedad, modalidades de venta y amenidades.
* **Gestión de Administradores y Desarrolladores:** Asignación de credenciales y permisos.

---

## 🎬 Guía Rápida de Demostración (Credenciales de Prueba)

| Rol de Usuario | Correo Electrónico | Contraseña | Objetivo de la Demostración |
| :--- | :--- | :--- | :--- |
| **🏢 Administrador** | `adminuser@realestate.com` | `Admin123!` | Ver Dashboard de KPIs, reasignar propiedades y gestionar catálogos. |
| **👔 Agente Inmobiliario** | `agentuser@realestate.com` | `Agent123!` | Publicar inmuebles con fotos, aceptar ofertas (regla atómica) y chatear. |
| **🛒 Cliente Comprador** | `clientuser@realestate.com` | `Client123!` | Enviar ofertas, simular hipoteca en RD$, agendar citas y chatear. |
| **💻 Desarrollador API** | `developeruser@realestate.com` | `Developer123!` | Consumir los servicios REST expuestos mediante JWT Bearer Token. |

---

## 📋 Instrucciones de Ejecución Local

### 1. Iniciar el Backend (.NET Web API)
```powershell
dotnet run --project src/Presentation/RealEstateApp.Presentation.WebApi
```
* **Swagger UI:** `http://localhost:5196/swagger`

### 2. Iniciar el Frontend (React SPA)
```powershell
cd src/Presentation/RealEstateApp.ClientApp
npm install
npm run dev
```
* **Aplicación Web:** `http://localhost:5173`

### 3. Ejecutar Pruebas Unitarias
* **Backend (.NET xUnit):** `dotnet test`
* **Frontend (Vitest):** `cd src/Presentation/RealEstateApp.ClientApp && npm test`

---

© 2026 **RealEstateApp** — Ecosistema Inmobiliario de Alto Rendimiento. Todos los derechos reservados.
