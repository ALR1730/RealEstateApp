# ⚙️ Guía de Instalación, Configuración y Despliegue — RealEstateApp V2

Esta guía describe los pasos necesarios para configurar, ejecutar y desplegar el ecosistema desacoplado de **RealEstateApp** (Frontend SPA en **React + Vite + Tailwind CSS** y Backend REST en **.NET 10 / ASP.NET Core Web API + SignalR** con base de datos **Microsoft SQL Server**).

---

## 🛠️ Stack Tecnológico de la Plataforma

* **Frontend:**
  * **Framework / Librería:** React 18 / 19 con TypeScript
  * **Bundler & Dev Server:** Vite 6
  * **Estilos y Diseño:** Tailwind CSS 3.4 (con paleta institucional de alta gama y utilidades Glassmorphism)
  * **Iconografía:** Lucide React
  * **Enrutamiento:** React Router DOM v6
  * **Comunicación HTTP:** Axios (con interceptores JWT Bearer automáticos)
  * **Tiempo Real:** `@microsoft/signalr` para chat en vivo y notificaciones WebSocket
  * **Pruebas Unitarias Frontend:** Vitest + React Testing Library

* **Backend:**
  * **Lenguaje:** C# 13
  * **Framework Principal:** .NET 10 / ASP.NET Core Web API
  * **Arquitectura:** Onion / Clean Architecture
  * **Persistencia y ORM:** Entity Framework Core (Code First con migraciones y semillas automáticas)
  * **Seguridad e Identidad:** ASP.NET Core Identity & JWT Bearer Tokens para todos los roles
  * **Servidor en Tiempo Real:** SignalR Hubs (`/hubs/chat` y `/hubs/notifications`)
  * **Pruebas Unitarias Backend:** xUnit + Moq + FluentAssertions
  * **Documentación API:** Swagger OpenAPI 3.0

* **Base de Datos:**
  * **Motor:** Microsoft SQL Server (LocalDB, Express o Azure SQL)

---

## 📋 Prerrequisitos del Sistema

Antes de iniciar la ejecución local, asegúrese de tener instalado:

1. [.NET 10 SDK o .NET 9 SDK](https://dotnet.microsoft.com/download)
2. [Node.js (v18 o superior)](https://nodejs.org/) y npm
3. [Microsoft SQL Server / LocalDB](https://www.microsoft.com/sql-server/)

---

## 🚀 Pasos para la Ejecución Local

### 1. Clonar el Repositorio
```bash
git clone https://github.com/tu-usuario/RealEstateApp.git
cd RealEstateApp
```

### 2. Configurar y Ejecutar el Backend (.NET Web API)

1. Restaurar dependencias y compilar la solución:
   ```bash
   dotnet restore
   dotnet build
   ```

2. Ejecutar la Web API:
   ```bash
   dotnet run --project src/Presentation/RealEstateApp.Presentation.WebApi
   ```
   * **URL de la API REST:** `http://localhost:5196` o `https://localhost:7196`
   * **Documentación Swagger UI:** `http://localhost:5196/swagger`
   * **Nota:** Al iniciar por primera vez, el sistema ejecutará automáticamente las migraciones de Entity Framework y sembrará los datos de prueba (usuarios, tipos de propiedades, amenidades e inmuebles).

### 3. Configurar y Ejecutar el Frontend (React SPA)

1. Navegar a la carpeta del cliente web:
   ```bash
   cd src/Presentation/RealEstateApp.ClientApp
   ```

2. Instalar las dependencias de Node:
   ```bash
   npm install
   ```

3. Iniciar el servidor de desarrollo de React:
   ```bash
   npm run dev
   ```
   * **URL de la Aplicación Web:** `http://localhost:5173`

---

## 🧪 Ejecución de Pruebas Unitarias

### Pruebas del Backend (.NET xUnit)
Desde la raíz del repositorio:
```bash
dotnet test
```
* Ejecuta las 25+ pruebas unitarias de controladores, reglas atómicas de ofertas, agendamiento de citas, servicios de divisas y simulador financiero.

### Pruebas del Frontend (Vitest)
Desde la carpeta `src/Presentation/RealEstateApp.ClientApp`:
```bash
npm test
```
* Valida las fórmulas financieras de amortización francesa en RD$, formateadores de moneda dominicana y renderizado de componentes.

---

## 🔑 Credenciales de Prueba Preconfiguradas (Seeding)

Para probar todas las experiencias de usuario, puede iniciar sesión de forma manual o con el panel de **1 Clic** en la pantalla de Login:

| Rol | Correo Electrónico | Contraseña | Funcionalidad Clave |
| :--- | :--- | :--- | :--- |
| **🏢 Administrador** | `admin@realestate.com` | `Admin123!` | Dashboard de KPIs, reasignación de agentes, CRUD de tipos y amenidades. |
| **👔 Agente Inmobiliario** | `agent@realestate.com` | `Agent123!` | Publicar inmuebles con fotos, aceptar ofertas con regla atómica, chat en vivo. |
| **🛒 Cliente Comprador** | `client@realestate.com` | `Client123!` | Guardar favoritos, simular cuotas en RD$, enviar ofertas y agendar visitas. |
| **💻 Desarrollador API** | `developer@realestate.com` | `Developer123!` | Acceso completo a los endpoints REST mediante JWT Bearer Token. |

---

© 2026 **RealEstateApp** — Ecosistema Inmobiliario de Alto Rendimiento.
