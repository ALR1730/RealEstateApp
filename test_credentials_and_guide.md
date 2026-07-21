# Guía de Datos de Prueba y Credenciales de Acceso — RealEstateApp V2

Este documento contiene la guía paso a paso y todas las **credenciales de prueba pre-configuradas** en la base de datos para probar manualmente y de manera exhaustiva el portal **Web MVC** y la **Web API RESTful (Swagger)**.

---

## 🚀 Paso 1: Iniciar las Aplicaciones y Cargar la Base de Datos

Al iniciar cualquiera de los proyectos (`WebApp` o `WebApi`), la aplicación ejecuta automáticamente todos los Seeds de inicialización en el primer arranque:

### Para ejecutar el Portal Web MVC:
```bash
dotnet run --project src/Presentation/RealEstateApp.Presentation.WebApp
```
- **URL WebApp MVC**: [http://localhost:5000](http://localhost:5000)

### Para ejecutar la Web API REST:
```bash
dotnet run --project src/Presentation/RealEstateApp.Presentation.WebApi
```
- **URL Swagger UI**: [http://localhost:5196](http://localhost:5196) (servido directamente en la raíz `/`)

---

## 🔑 Paso 2: Matriz de Credenciales de Prueba por Rol

Todos los usuarios de prueba vienen con **correo verificado (`EmailConfirmed = true`)** listo para usar.

| Rol | Correo Electrónico | Contraseña | Nombre de Usuario | Descripción / Uso Recomendado |
|-----|-------------------|------------|-------------------|-------------------------------|
| **Admin** | `admin@realestate.com` | `Admin123!` | `adminuser` | **Admin Principal**: Acceso al Dashboard de KPIs, gestión de agentes (activar/inactivar/borrado en cascada), alta de desarrolladores/admins y mantenimientos. |
| **Admin** | `admin2@realestate.com` | `Admin123!` | `adminuser2` | **Admin Secundario**: Cuenta de respaldo para pruebas de administración. |
| **Agente** | `agent@realestate.com` | `Agent123!` | `agentuser` | **Agente Activo 1**: Posee las propiedades `APT101` y `VIL202`. Tiene ofertas recibidas pendientes y chats activos con clientes. |
| **Agente** | `agent2@realestate.com` | `Agent123!` | `agentuser2` | **Agente Activo 2**: Posee las propiedades `CAS303` y `PNT404` (Vendida con oferta aceptada). |
| **Agente** | `agent3@realestate.com` | `Agent123!` | `agentuser3` | **Agente Pendiente / Inactivo**: Cuenta bloqueada por defecto para probar la **activación por el Administrador** o el intento de login bloqueado (`PendingActivation.cshtml`). |
| **Cliente** | `client@realestate.com` | `Client123!` | `clientuser` | **Cliente Principal**: Tiene `APT101` y `VIL202` en favoritos, una oferta enviada por $185,000 USD en `APT101` e hilos de chat iniciados. |
| **Cliente** | `client2@realestate.com` | `Client123!` | `clientuser2` | **Cliente Secundario**: Posee la oferta aceptada en `PNT404`. |
| **Desarrollador** | `developer@realestate.com` | `Developer123!` | `developeruser` | **Developer REST**: Uso exclusivo en Swagger Web API (`POST /api/v1/Account/authenticate` para obtener token JWT). Bloqueado en WebApp MVC por seguridad cruzada. |
| **Desarrollador** | `dev2@realestate.com` | `Developer123!` | `developeruser2` | **Developer Secundario**: Cuenta alternativa para pruebas de API. |

---

## 🏡 Paso 3: Datos de Prueba Pre-Cargados en la BD

### 1. Propiedades de Ejemplo Publicadas

| Código | Tipo de Inmueble | Tipo de Venta | Precio Lista | Estado | Agente Propietario | Características |
|--------|------------------|---------------|--------------|--------|--------------------|-----------------|
| **`APT101`** | Apartamento | Venta Directa | $195,000 USD | **Disponible** | `agent@realestate.com` | 3 Habs, 2 Baños, 135.5 m², Planta Eléctrica, Ascensor, Seguridad 24/7. |
| **`VIL202`** | Villa | Venta Directa | $450,000 USD | **Disponible** | `agent@realestate.com` | 5 Habs, 4 Baños, 380 m², Piscina, Planta Eléctrica, Seguridad 24/7. |
| **`CAS303`** | Casa | Alquiler | $280,000 USD | **Disponible** | `agent2@realestate.com` | 4 Habs, 3 Baños, 250 m², Patio trasero y terraza. |
| **`PNT404`** | Penthouse | Venta Directa | $520,000 USD | **Vendida** | `agent2@realestate.com` | 4 Habs, 4 Baños, 310 m², Jacuzzi privado, Ascensor, Piscina. |

### 2. Ofertas de Prueba Activas
- **Oferta 1**: `client@realestate.com` en `APT101` por **$185,000 USD** (Estado: `Pending`).
  - *Prueba recomendada*: Entrar como `agent@realestate.com` a "Ofertas Recibidas" y hacer clic en **Aceptar**. Se ejecutará la **Regla Atómica**: la propiedad pasará a *Vendida* y se rechazarán las demás propuestas competidoras.
- **Oferta 2**: `client2@realestate.com` en `PNT404` por **$510,000 USD** (Estado: `Accepted`).

### 3. Chats e Hilos de Mensajería
- Conversación activa pre-creada entre **`client@realestate.com`** y **`agent@realestate.com`** en torno al inmueble **`APT101`** con 3 mensajes guardados en el historial.

---

## 🧪 Paso 4: Guía de Pruebas Manuales Recomendadas

### 1. Prueba de Seguridad Cruzada
- **Intento de Login de Developer en WebApp**: Iniciar sesión en `http://localhost:5000/Account/Login` con `developer@realestate.com` / `Developer123!`.
  - *Resultado Esperado*: Muestra mensaje de error restringiendo el acceso MVC a cuentas Developer.
- **Intento de Autenticación de Cliente en Web API**: Ejecutar `POST /api/v1/Account/authenticate` en Swagger con `client@realestate.com` / `Client123!`.
  - *Resultado Esperado*: Retorna `400 Bad Request` con mensaje de acceso denegado.

### 2. Prueba del Flujo de Agente y Regla Atómica
1. Iniciar sesión como `agent@realestate.com` (`Agent123!`).
2. Ir a **"Ofertas Recibidas"**.
3. Presionar **Aceptar** en la oferta de `APT101`.
4. Verificar que la propiedad en **"Mis Inmuebles"** cambia a estado **Vendida**.

### 3. Prueba de Eliminación en Cascada por el Administrador
1. Iniciar sesión como `admin@realestate.com` (`Admin123!`).
2. Ir a **Dashboard** (revisar contadores de KPIs) y hacer clic en **"Agentes"**.
3. Probar el botón **Inactivar / Activar** en `agent3@realestate.com`.
4. Probar la **Eliminación Física en Cascada** sobre un agente y verificar que elimina todas sus propiedades, favoritos, chats e imágenes vinculadas.

### 4. Prueba del Cliente y Simulador Hipotecario
1. Iniciar sesión como `client@realestate.com` (`Client123!`).
2. Ir a **"Mis Favoritos"** o entrar a la propiedad `APT101`.
3. Ajustar el monto de inicial, tasa de interés y plazo en el **Simulador Hipotecario** y presionar **Calcular Cuota**.
4. Ir a **"Mis Chats"** y enviar un nuevo mensaje a `agent@realestate.com`.
