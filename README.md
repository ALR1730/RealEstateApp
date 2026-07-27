# 🏢 RealEstateApp — Plataforma Integral de Gestión e Intermediación Inmobiliaria 🏠💼

[![Moneda](https://img.shields.io/badge/Moneda-RD%24%20Pesos%20Dominicanos-blue.svg)]()
[![Tecnología](https://img.shields.io/badge/.NET-10.0%20%2F%20ASP.NET%20Core-purple.svg)](https://dotnet.microsoft.com/)
[![Arquitectura](https://img.shields.io/badge/Arquitectura-Empresarial%20Onion-orange.svg)]()
[![Estado](https://img.shields.io/badge/Estado-Listo%20para%20Producci%C3%B3n-brightgreen.svg)]()

---

## 📌 Resumen Ejecutivo para la Alta Dirección

**RealEstateApp** es un ecosistema tecnológico transaccional de última generación diseñado específicamente para **firmas inmobiliarias, desarrolladoras de proyectos y corredores de bienes raíces**. 

La plataforma transforma el flujo tradicional de catálogos pasivos en una **plataforma interactiva de aceleración de ventas**, automatizando el ciclo completo de negocio: desde la captación del prospecto y la simulación crediticia en **Pesos Dominicanos (RD$)**, hasta la negociación de ofertas en tiempo real y el cierre de ventas garantizado sin riesgos de duplicidad.

---

## 💡 Propuesta de Valor y Ventajas de Negocio

### 🎯 1. Aceleración del Cierre de Ventas y Automatización de Ofertas (Regla Atómica)
- **Eliminación del Error Humano:** Al aceptar una propuesta económica, el sistema cambia instantáneamente el estado del inmueble a **"Vendida"**, rechazando automáticamente en cascada las demás ofertas pendientes y bloqueando nuevas solicitudes.
- **Acceso Exclusivo al Comprador:** El comprador que cierra la negociación conserva acceso exclusivo a la propiedad para continuar la comunicación con su agente, manteniendo la privacidad hacia el público general.

### 💰 2. Simulador Hipotecario Profesional en Pesos Dominicanos (RD$)
- **Empoderamiento Financiero del Comprador:** Herramienta interactiva adaptada al mercado de República Dominicana con cálculo de cuota fija mensual bajo el **Sistema de Amortización Francés**.
- **Desglose Transparente:** Muestra gráfica del enganche/inicial, capital prestado e intereses totales acumulados, permitiendo al cliente **imprimir o descargar su tabla amortizada en PDF** antes de realizar una oferta.

### 💬 3. Comunicación Directa y Retención de Prospectos (Leads)
- **Chat Privado por Inmueble:** Cada propiedad cuenta con su propio hilo de mensajería directa entre el cliente interesado y el agente asignado, eliminando la fuga de prospectos a canales externos sin trazabilidad.

### 📊 4. Control Directivo y Cuadro de Mando Ejecutivo (Dashboard KPIs)
- **Visibilidad 360° en Tiempo Real:** Métricas clave de propiedades (Disponibles, Reservadas, Vendidas) y fuerza de ventas (Agentes Activos/Inactivos).
- **Gestión Flexibilizada:** Permite a la gerencia reasignar inmuebles entre agentes o retirar publicaciones con un solo clic.

---

## 👥 Experiencia por Perfil de Usuario (Flujos de Trabajo)

### 🛒 Para el Cliente / Inversionista (Comprador)
* **Búsqueda Avanzada:** Filtrado multicriterio por tipo de propiedad (Apartamento, Villa, Penthouse, Casa), tipo de venta, rango de precios (RD$), habitaciones y baños.
* **Simulación de Crédito Dinámica:** Ajuste de plazo (5 a 30 años) y tasa de interés anual para calcular la capacidad de pago.
* **Módulo de Ofertas y Favoritos:** Historial transparente con estado de ofertas (*Pendiente, Aceptada, Rechazada*) y panel de favoritos actualizado.

### 👔 Para el Agente Inmobiliario (Corredor)
* **Gestión de Publicaciones:** Carga de portafolio con hasta 15 fotografías, geolocalización de Google Maps, enlaces a Video Tour y Tour Virtual 360°.
* **Bandeja de Negociaciones:** Visualización de propuestas recibidas y gestión de clientes mediante hilos de mensajería organizados por propiedad.

### 🏢 Para la Dirección Inmobiliaria (Administrador)
* **Gobierno de la Plataforma:** Control total sobre corredores (activación, inactivación y eliminación segura), usuarios desarrolladores y administradores.
* **Reasignación de Cartera:** Transferencia ágil de propiedades entre agentes ante cambios de personal.
* **Mantenimiento de Catálogos Núcleo:** Gestión de categorías de propiedades, modalidades de negocio y amenidades/mejoras.

---

## 📱 Ecosistema Digital Escalable (Web API JWT)

Para empresas que buscan expandirse a aplicaciones móviles (iOS / Android) o integrarse con portales inmobiliarios internacionales (MLS):

- **API REST Protegida:** Endpoints seguros autenticados mediante **Tokens JWT (JSON Web Tokens)**.
- **Documentación Swagger UI:** Interfaz gráfica interactiva para que equipos de desarrollo conecten fácilmente el sistema con CRM externos o aplicaciones nativas.

---

## 🎬 Guía para Demostración Interactiva (Demo Rápido)

Para visualizar el funcionamiento en vivo de la plataforma, utilice cualquiera de los siguientes perfiles de prueba preconfigurados:

| Rol de Usuario | Correo Electrónico (Login) | Contraseña | Objetivo de la Demostración |
| :--- | :--- | :--- | :--- |
| **🏢 Administrador** | `adminuser@realestate.com` | `Admin123!` | Ver Dashboard de KPIs, reasignación de propiedades y gestión global. |
| **👔 Agente Inmobiliario** | `agentuser@realestate.com` | `Agent123!` | Ver inmuebles del portafolio, aceptar ofertas y responder chats. |
| **🛒 Cliente Comprador** | `clientuser@realestate.com` | `Client123!` | Enviar ofertas, simular hipoteca en RD$ y chatear con agentes. |
| **💻 Desarrollador API** | `developeruser@realestate.com` | `Developer123!` | Consultar servicios web expuestos mediante JWT. |

---

## 🛠️ Arquitectura Técnica de Clase Empresarial

El sistema está construido bajo los más altos estándares de ingeniería de software mediante la **Arquitectura Onion (Clean Architecture)** en **.NET 10**:

```text
└── RealEstateApp
    ├── RealEstateApp.Core.Domain          # Entidades de negocio puras (Propiedades, Ofertas, Chats, Usuarios).
    ├── RealEstateApp.Core.Application     # Lógica de aplicación, simulador financiero, contratos de repositorios y DTOs.
    ├── RealEstateApp.Infrastructure.Persistence  # Base de datos SQL Server mediante EF Core con Code First y Eager Loading.
    ├── RealEstateApp.Infrastructure.Shared       # Servicios de infraestructura compartida (Cultura es-DO, Simulador Francés).
    ├── RealEstateApp.Presentation.WebApp         # Aplicación Web MVC con diseño responsivo, glassmorphism y Bootstrap 5.
    └── RealEstateApp.Presentation.WebApi         # Servicios Web REST expuestos con seguridad JWT y Swagger UI.
```

---

## 📋 Instrucciones de Despliegue y Ejecución

1. **Requisitos Previos:** tener instalado [.NET 9 SDK o .NET 10 SDK](https://dotnet.microsoft.com/) y SQL Server / LocalDB.
2. **Ejecución de la Aplicación Web:**
   ```powershell
   dotnet run --project src/Presentation/RealEstateApp.Presentation.WebApp
   ```
3. **Acceso en el Navegador:** Navegue a `http://localhost:5000` o la URL configurada en consola. La base de datos y los datos iniciales de prueba se inicializarán automáticamente.

---

© 2026 **RealEstateApp** — Solución Tecnológica para el Sector Inmobiliario. Todos los derechos reservados.
