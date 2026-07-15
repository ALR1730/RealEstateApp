# RealEstateApp 🏠💼

[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Onion-orange.svg)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)]()

**RealEstateApp** es una plataforma web de nivel empresarial, robusta y altamente escalable diseñada para modernizar y optimizar el mercado inmobiliario digital. Este ecosistema conecta de forma fluida a **Clientes, Agentes Inmobiliarios y Administradores**, transformando el flujo tradicional de catálogos estáticos en una experiencia completamente interactiva y transaccional que abarca mensajería interna, ofertas estructuradas y simulación de financiamiento bancario.

El proyecto implementa con rigurosidad los mejores estándares de la industria, destacando una separación absoluta de responsabilidades mediante **Onion Architecture (100% consistente)** y la exposición segura de servicios estructurados a través de una **Web API protegida por JSON Web Tokens (JWT)**.

---

## 🚀 Características Principales

### 👤 Módulo Público (Visitantes)
* **Catálogo Inmobiliario Dinámico:** Listado en tiempo real de propiedades disponibles ordenadas de forma cronológica inversa (de la más reciente a la más antigua).
* **Búsqueda por Código Único:** Formulario de consulta inmediata mediante un identificador numérico único de 6 dígitos generado automáticamente por el sistema.
* **Filtros Avanzados Combinables:** Motores de filtrado paralelos por categoría/tipo de propiedad, rangos de precio (DOP), cantidad de habitaciones y cantidad de baños.
* **Directorio de Agentes Públicos:** Catálogo organizado alfabéticamente de agentes activos con acceso directo al portafolio exclusivo de sus inmuebles disponibles.

### 🛍️ Módulo de Clientes (Autenticados)
* **Gestión de Favoritos:** Panel personalizado ("Mis Propiedades") para el marcaje, seguimiento y desmarcaje de inmuebles de interés. Las propiedades vendidas se depuran automáticamente del listado.
* **Módulo de Chat Integrado:** Canal bidireccional y privado asociado a cada propiedad para interactuar directamente con el agente responsable.
* **Estructura de Ofertas y Financiamiento:** Envío formal de propuestas económicas especificando montos, cálculo automático de cuotas y planes de financiamiento. Historial transparente con estados dinámicos (*Pendiente, Aceptada, Rechazada*).

### 👔 Módulo de Agentes Inmobiliarios
* **Control de Portafolio Inmobiliario:** Mantenimiento completo (CRUD) de propiedades asociando múltiples imágenes, descripciones detalladas, tipos de operaciones y mejoras estructurales del inmueble.
* **Automatización de Reglas de Negocio:** Panel central de ofertas recibidas. Al aceptar una propuesta, el sistema actualiza de forma atómica el estado del inmueble a *Reservado/Vendido*, rechaza masivamente el resto de ofertas competidoras para evitar errores humanos y deshabilita nuevas solicitudes.
* **Bandeja de Conversaciones:** Gestión ordenada de hilos de mensajería segmentados por cliente y propiedad específica.

### ⚙️ Módulo de Administración (Backoffice)
* **Dashboard de Indicadores Globales:** Panel ejecutivo con contadores exactos de propiedades disponibles frente a vendidas, y desglose de usuarios activos/inactivos por rol.
* **Mantenimientos de Catálogos Núcleo:** Gestión dinámica de tipos de propiedades, tipos de ventas/operaciones y mejoras estructurales requeridas por la capa de negocio.
* **Auditoría y Gestión de Cuentas:** Activación, inactivación y borrado físico/lógico de agentes en cascada, garantizando la eliminación limpia de registros huérfanos o inconsistencias en cascada (favoritos, ofertas, chats e imágenes).

### 🌐 Web API y Seguridad Externa (Desarrolladores)
* **Endpoints Protegidos:** Controladores REST optimizados (`PropertyController`, `AgentController`, etc.) para interactuar externamente con la data maestra de la plataforma.
* **Autenticación Basada en Tokens:** Implementación estricta de seguridad con **JWT utilizando el esquema Bearer**.
* **Documentación Viva:** Integración con **Swagger UI** para la exploración interactiva y autogeneración de contratos de servicios.

---

## 🏗️ Arquitectura del Software

El ecosistema se rige al 100% bajo el patrón arquitectónico **Onion Architecture** (Arquitectura de Cebolla), aislando por completo el núcleo del negocio y las reglas de aplicación frente a frameworks, sistemas de bases de datos o la interfaz de usuario.

```text
└── RealEstateApp
    ├── RealEstateApp.Core.Domain          # Entidades de dominio puras, Enums, configuraciones de negocio y POCOs.
    ├── RealEstateApp.Core.Application     # Lógica de aplicación, Interfaces de Servicios/Repositorios, DTOs, ViewModels, perfiles de AutoMapper y validaciones del sistema.
    ├── RealEstateApp.Infrastructure.Persistence  # DbContext (Entity Framework Core Code First), Repositorios Genéricos/Específicos, inicializadores de datos (Seeds) y Migraciones.
    ├── RealEstateApp.Infrastructure.Shared       # Implementaciones de infraestructura cruzada: IEmailService, Storage de imágenes en la nube (AWS S3/Azure Blobs), pasarelas de pago y GIS.
    ├── RealEstateApp.Presentation.WebApp         # Interfaz de usuario final basada en ASP.NET Core MVC 9, Bootstrap 5 y arquitectura de ViewModels limpios.
    └── RealEstateApp.Presentation.WebApi         # Servicios REST (Controladores de API expuestos, políticas JWT y middlewares de Swagger).
