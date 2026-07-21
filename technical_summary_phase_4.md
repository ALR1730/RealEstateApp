# Ficha Técnica del Proyecto: RealEstateApp V2 — Fase 4 (Interfaz de Usuario y Experiencia del Cliente MVC)

Este documento contiene un resumen técnico exhaustivo y de bajo nivel de todos los cambios, archivos creados, modificados y configurados durante la **Fase 4: Interfaz de Usuario y Experiencia del Cliente (Presentation.WebApp - MVC)** del proyecto **RealEstateApp V2**. Está estructurado para ser leído y procesado por cualquier Modelo de Lenguaje (IA) para su estudio o explicación al detalle.

---

## 🏗️ 1. Alcance de la Fase 4

La Fase 4 construyó el **portal web cliente y visitante** en la capa `RealEstateApp.Presentation.WebApp` utilizando ASP.NET Core MVC, Bootstrap 5 y un sistema de diseño propio basado en Vanilla CSS e inspirado en **shadcn/ui**.

```
RealEstateApp (Solution)
 ├── src
 │    ├── Core
 │    │    ├── RealEstateApp.Core.Domain         ✅ Sin cambios (Fase 1/3 completa)
 │    │    └── RealEstateApp.Core.Application    ⭐ Agregado LoginViewModel, RegisterViewModel
 │    ├── Infrastructure
 │    │    ├── RealEstateApp.Infrastructure.Persistence ✅ Sin cambios (Fase 3 completa)
 │    │    └── RealEstateApp.Infrastructure.Shared      ✅ Sin cambios (Fase 2 completa)
 │    └── Presentation
 │         ├── RealEstateApp.Presentation.WebApi        ✅ Sin cambios (Fase 3 completa)
 │         └── RealEstateApp.Presentation.WebApp        ⭐ FOCO PRINCIPAL (Controllers MVC, Views Razor, site.css)
```

---

## 💾 2. Cambios en Detalle por Capas

### Capa 2.1: Core Application (`RealEstateApp.Core.Application`)

#### ViewModels de Identidad (`ViewModels/Account/`)
1. **`LoginViewModel.cs`** (Nuevo):
   - Campos: `Email` (`[Required]`, `[Display(Name = "Correo o Nombre de usuario")]`), `Password` (`[Required]`, `[DataType(DataType.Password)]`), `RememberMe` (`bool`), `ReturnUrl` (`string?`).
2. **`RegisterViewModel.cs`** (Nuevo):
   - Campos: `FirstName` (`[Required]`, `[StringLength(100)]`), `LastName` (`[Required]`, `[StringLength(100)]`), `Email` (`[Required]`, `[EmailAddress]`), `UserName` (`[Required]`, `[StringLength(50, Min=3)]`), `Password` (`[Required]`, `[StringLength(100, Min=6)]`), `ConfirmPassword` (`[Required]`, `[Compare(Password)]`), `Phone` (`[Phone]`), `UserType` (`[Required]`, default `"Client"`, opciones: `"Client"` o `"Agent"`).

---

### Capa 2.2: Presentation WebApp — Controladores (`Controllers/`)

1. **`AccountController.cs`** (Nuevo):
   - Inyecta `IAccountService`, `SignInManager<IdentityUser>`, `UserManager<IdentityUser>`.
   - `GET /Account/Login`: Renderiza formulario de inicio de sesión.
   - `POST /Account/Login`: Valida credenciales, comprueba si el correo fue confirmado y si la cuenta no está bloqueada por lockout. Invoca `PasswordSignInAsync()`.
   - `GET /Account/Register`: Renderiza formulario de registro con selector de tipo de cuenta.
   - `POST /Account/Register`: Invoca `_accountService.RegisterUserAsync()`.
     - Si es **Cliente**: Se genera token de confirmación y correo electrónico. Redirige a `ConfirmEmailResult.cshtml`.
     - Si es **Agente**: Nace bloqueado/inactivo por defecto. Redirige a `PendingActivation.cshtml`.
   - `GET /Account/ConfirmEmail`: Valida el token y activa la cuenta del cliente.
   - `POST /Account/Logout`: Invoca `_signInManager.SignOutAsync()`.

2. **`HomeController.cs`** (Modificado):
   - Inyecta `IPropertyService`, `IPropertyTypeService`, `ISaleTypeService`, `IFavoriteService`, `IFinancingService`, `UserManager<IdentityUser>`.
   - `GET /`: Carga propiedades mediante `GetAllWithFilters(filters)`. Pobla dropdowns mapeando los tipos de propiedad y venta a los ViewModels de filtro. Si el cliente está autenticado, resuelve sus favoritos marcando `IsFavorite = true`.
   - `GET /Home/Details/{id}`: Obtiene el detalle de la propiedad, resuelve el nombre del agente y si es favorita para el usuario actual.
   - `POST /Home/CalculateMortgage`: Endpoint AJAX que invoca `_financingService.GenerateAmortizationSchedule()` y retorna JSON con la tabla de amortización.

3. **`AgentsController.cs`** (Nuevo):
   - Inyecta `IAgentService`.
   - `GET /Agents`: Directorio público de agentes activos (`IsActive == true`).
   - `GET /Agents/Details/{id}`: Detalle del agente y su portafolio completo de inmuebles.

4. **`FavoritesController.cs`** (Nuevo):
   - Atributo `[Authorize(Roles = "Client")]`.
   - `GET /Favorites`: Lista propiedades favoritas del cliente (la capa Persistence filtra automáticamente las vendidas).
   - `POST /Favorites/Toggle`: Agrega o elimina una propiedad de favoritos.

5. **`OffersController.cs`** (Nuevo):
   - Atributo `[Authorize(Roles = "Client")]`.
   - `GET /Offers/Create?propertyId=...` & `POST /Offers/Create`: Muestra y procesa la propuesta económica. Procesa la subida de la carta de pre-aprobación bancaria (`IFormFile`) mediante `IFileStorageService`.
   - `GET /Offers/MyOffers`: Consulta las ofertas enviadas por el cliente.

6. **`ChatsController.cs`** (Nuevo):
   - Atributo `[Authorize]`.
   - `GET /Chats`: Listado de conversaciones activas agrupadas por propiedad.
   - `GET /Chats/Thread?propertyId=...&agentId=...`: Hilo directo de mensajes entre cliente y agente para una propiedad.
   - `POST /Chats/Send`: Envía un nuevo mensaje en el hilo.

---

### Capa 2.3: Presentation WebApp — Vistas Razor (`Views/`)

| Vista | Ruta | Descripción / Elementos Destacados |
|-------|------|------------------------------------|
| `_Layout.cshtml` | `Views/Shared/` | Header glassmorphic, navegación dinámica por rol, Google Fonts Inter, Bootstrap Icons, footer. |
| `Login.cshtml` | `Views/Account/` | Tarjeta con sombra, íconos de entrada, RememberMe, validaciones. |
| `Register.cshtml` | `Views/Account/` | Selector de rol interactivo (Cliente vs Agente), grilla de dos columnas. |
| `ConfirmEmailResult.cshtml` | `Views/Account/` | Notificación de activación de cuenta. |
| `PendingActivation.cshtml` | `Views/Account/` | Mensaje informativo para agentes pendientes de aprobación administrativa. |
| `AccessDenied.cshtml` | `Views/Account/` | Pantalla de acceso denegado por rol. |
| `Index.cshtml` | `Views/Home/` | Hero Banner, Buscador rápido por código de 6 dígitos, Filtros combinados, Grilla de tarjetas con zoom en hover y badges de estado. |
| `Details.cshtml` | `Views/Home/` | Carrusel de galería, mapa, video, tour 360, mejoramientos, simulador de hipoteca interactivo JS, acciones de cliente. |
| `Index.cshtml` | `Views/Agents/` | Directorio de agentes con foto/avatar y datos. |
| `Details.cshtml` | `Views/Agents/` | Portafolio completo de propiedades del agente. |
| `Index.cshtml` | `Views/Favorites/` | Grilla de favoritos guardados. |
| `Create.cshtml` | `Views/Offers/` | Formulario de oferta con análisis dinámico de precio en JS y subida de archivo. |
| `MyOffers.cshtml` | `Views/Offers/` | Tabla con historial de ofertas y badges de estado (`Pending`, `Accepted`, `Rejected`). |
| `Index.cshtml` | `Views/Chats/` | Tarjetas de conversaciones agrupadas por inmueble. |
| `Thread.cshtml` | `Views/Chats/` | Interfaz de mensajería con burbujas emisor/receptor y scroll automático. |

---

## 🎨 3. Sistema de Diseño CSS (`wwwroot/css/site.css`)

- **Paleta de Colores**:
  - `Primary Dark`: `#0f172a` (Azul Marino Profundo)
  - `Accent Blue`: `#0284c7` (Azul Cielo)
  - `Accent Emerald`: `#10b981` (Verde Esmeralda)
  - `Accent Amber`: `#f59e0b` (Ámbar)
  - `Accent Rose`: `#f43f5e` (Rosa/Rojo)
- **Glassmorphism**: `backdrop-filter: blur(16px); background: rgba(15, 23, 42, 0.92);`.
- **Badges de Estado**:
  - `badge-disponible`: Fondo verde esmeralda `#10b981`.
  - `badge-reservada`: Fondo ámbar `#f59e0b`.
  - `badge-vendida`: Fondo rosa `#f43f5e`.
- **Micro-interacciones**: Zoom sutil en hover sobre imágenes de propiedad, elevación de tarjetas con sombra extendida, efecto activo en botón de favoritos.

---

## 🔍 4. Resultados de la Verificación

```
> dotnet build src/Presentation/RealEstateApp.Presentation.WebApp/RealEstateApp.Presentation.WebApp.csproj
Build succeeded.
    9 Warning(s)   ← Pre-existentes
    0 Error(s)
```

- El proyecto WebApp compila sin ningún error de código C#.
