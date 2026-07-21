# Ficha Técnica del Proyecto: RealEstateApp V2 — Fase 5 (Panel del Agente y del Administrador MVC)

Este documento contiene un resumen técnico exhaustivo y de bajo nivel de todos los cambios, archivos creados, modificados y configurados durante la **Fase 5: Panel del Agente y del Administrador (Presentation.WebApp - MVC)** del proyecto **RealEstateApp V2**. Está estructurado para ser leído y procesado por cualquier Modelo de Lenguaje (IA) para su estudio o explicación al detalle.

---

## 🏗️ 1. Alcance de la Fase 5

La Fase 5 implementó el módulo de negocio completo para **Agente Inmobiliario (`Agent`)**, el Dashboard y módulo de **Administrador (`Admin`)**, y los mantenimientos de catálogo compartidos con el **Desarrollador (`Developer`)**.

```
RealEstateApp (Solution)
 ├── src
 │    ├── Core
 │    │    ├── RealEstateApp.Core.Domain         ✅ Sin cambios
 │    │    └── RealEstateApp.Core.Application    ⭐ Actualizado AgentService, PropertyService, SavePropertyViewModel
 │    ├── Infrastructure
 │    │    ├── RealEstateApp.Infrastructure.Persistence ✅ Sin cambios
 │    │    └── RealEstateApp.Infrastructure.Shared      ✅ Sin cambios
 │    └── Presentation
 │         ├── RealEstateApp.Presentation.WebApi        ✅ Sin cambios
 │         └── RealEstateApp.Presentation.WebApp        ⭐ Modulo Agente, Modulo Admin, Mantenimientos
```

---

## 💾 2. Cambios en Detalle por Capas

### Capa 2.1: Core Application (`RealEstateApp.Core.Application`)

1. **`IAgentService.cs` & `AgentService.cs`**:
   - `DeleteAgentCascadeAsync(string agentId)`: Orquesta la eliminación física en cascada:
     - Elimina archivos físicos de imágenes en disco (`/uploads/properties/`).
     - Elimina registros de `PropertyImage`, `Favorite`, `Offer`, `Chat` y `Property` vinculados a sus inmuebles.
     - Elimina cualquier chat residual del agente.
     - Elimina la cuenta en ASP.NET Core Identity.

2. **`IPropertyService.cs` & `PropertyService.cs`**:
   - Soporte para subida múltiple de hasta 15 imágenes usando `IFileStorageService.UploadFileAsync`.
   - `GetByIdSaveViewModel(int id)`: Mapea datos y llena `ExistingImages` para edición.
   - `DeleteImage(int imageId)`: Borra la imagen en disco y en BD.

---

### Capa 2.2: Presentation WebApp — Controladores (`Controllers/`)

1. **`AgentController.cs`** (`[Authorize(Roles = "Agent")]`):
   - `GET /Agent/Properties`: Lista propiedades del agente logueado.
   - `GET/POST /Agent/CreateProperty`: Creación de propiedad con subida de 15 fotos, geolocalización, video, tour 360 y mejoramientos.
   - `GET/POST /Agent/EditProperty`: Edición de propiedades.
   - `POST /Agent/DeleteProperty`: Eliminación de propiedad.
   - `GET /Agent/Offers`: Panel de ofertas recibidas.
   - `POST /Agent/AcceptOffer`: Executa **Regla Atómica**: Acepta oferta → Propiedad pasa a *Vendida* → Rechaza en cascada ofertas pendientes competidoras.
   - `POST /Agent/RejectOffer`: Rechaza oferta individual.

2. **`AdminController.cs`** (`[Authorize(Roles = "Admin")]`):
   - `GET /Admin/Dashboard`: KPIs en tiempo real (propiedades disponibles/reservadas/vendidas, agentes activos/inactivos, clientes y desarrolladores).
   - `GET /Admin/Agents`: Tabla de agentes con toggle de estado.
   - `POST /Admin/ToggleAgentStatus`: Activa o inactiva un agente.
   - `GET/POST /Admin/DeleteAgent`: Diálogo de confirmación y ejecución de la eliminación física en cascada.
   - `GET/POST /Admin/Developers` & `CreateDeveloper`: Creación de desarrolladores.
   - `GET/POST /Admin/Admins` & `CreateAdmin`: Creación de administradores.

3. **Controladores de Mantenimiento (`[Authorize(Roles = "Admin,Developer")]`)**:
   - **`AdminPropertyTypesController.cs`**: CRUD de Tipos de Propiedad.
   - **`AdminSaleTypesController.cs`**: CRUD de Tipos de Venta.
   - **`AdminImprovementsController.cs`**: CRUD de Mejoras.

---

### Capa 2.3: Presentation WebApp — Vistas Razor (`Views/`)

| Vista | Ruta | Descripción |
|-------|------|-------------|
| `Properties.cshtml` | `Views/Agent/` | Grilla de propiedades publicadas por el agente. |
| `CreateProperty.cshtml` | `Views/Agent/` | Formulario con subida múltiple de 15 fotos, lat/long y mejoramientos. |
| `EditProperty.cshtml` | `Views/Agent/` | Edición de propiedades con vista previa de imágenes. |
| `Offers.cshtml` | `Views/Agent/` | Panel de ofertas recibidas con botones para regla atómica. |
| `Dashboard.cshtml` | `Views/Admin/` | Tarjetas de KPI e indicadores del sistema. |
| `Agents.cshtml` | `Views/Admin/` | Mantenimiento de agentes con switches de estado y eliminación. |
| `DeleteAgent.cshtml` | `Views/Admin/` | Confirmación de borrado en cascada. |
| `Developers.cshtml` & `CreateDeveloper.cshtml` | `Views/Admin/` | Gestión de cuentas de desarrollador. |
| `Admins.cshtml` & `CreateAdmin.cshtml` | `Views/Admin/` | Gestión de cuentas de administrador. |
| `Index.cshtml`, `Create.cshtml`, `Edit.cshtml` | `Views/AdminPropertyTypes/` | CRUD de Tipos de Propiedad. |
| `Index.cshtml`, `Create.cshtml`, `Edit.cshtml` | `Views/AdminSaleTypes/` | CRUD de Tipos de Venta. |
| `Index.cshtml`, `Create.cshtml`, `Edit.cshtml` | `Views/AdminImprovements/` | CRUD de Mejoras. |

---

## 🔍 3. Resultados de la Verificación

- Compilación limpia en C#: **0 Errores sintácticos en el código del proyecto**.
