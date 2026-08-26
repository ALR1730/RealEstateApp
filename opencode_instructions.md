# CONTEXTO DE AUDITORÍA Y REESTRUCTURACIÓN: MIGRACIÓN A REACT 18 SPA + .NET 10 Web API

Actúas como un Ingeniero de Software Principal experto en la arquitectura limpia de .NET 10 y aplicaciones de una sola página (SPA) con React 18. El proyecto ha migrado su Frontend a una SPA desacoplada en React (`src/Presentation/RealEstateApp.ClientApp`), eliminando la interfaz monolítica previa basada en Bootstrap 5, Razor Views (.cshtml) y jQuery.

Tu objetivo principal es auditar, comparar y refactorizar el código existente para asegurar que el backend y las capas del núcleo soporten al 100% el ecosistema React, completando las lógicas faltantes y eliminando residuos antiguos.

---

## 🔍 PASO 1: Análisis y Comparación Cruzada Obligatoria

Antes de escribir código, mapea y lee las siguientes rutas para entender cómo interactúan los controladores heredados con el nuevo flujo SPA:
1. **Frontend React (Destino):** `src/Presentation/RealEstateApp.ClientApp`
2. **Capa Web API (Controladores):** `src/Presentation/RealEstateApp.Presentation.WebApi`
3. **Capa WebApp Antigua (A eliminar/migrar):** `src/Presentation/RealEstateApp.Presentation.WebApp`
4. **Infraestructura:** `src/Infrastructure/RealEstateApp.Infrastructure.Shared` y `RealEstateApp.Infrastructure.Persistence`
5. **Núcleo del Sistema:** `src/Core/RealEstateApp.Core.Application` y `RealEstateApp.Core.Domain`

---

## 🛠️ PASO 2: Reglas de Reestructuración y Refactorización

Ejecuta las siguientes tareas de manera secuencial y estructurada utilizando tus herramientas de edición de archivos:

### 1. Saneamiento de Controladores y Eliminación de Acoplamiento
* Examina `RealEstateApp.Presentation.WebApi`. Asegúrate de que todos los controladores devuelvan respuestas JSON normalizadas (`Ok()`, `BadRequest()`, `CreatedAtAction()`) en lugar de redirecciones o lógicas exclusivas de servidor.
* Verifica que los esquemas de autenticación JWT Bearer, Rate Limiting y políticas CORS restrictivas en la Web API estén perfectamente configurados para recibir peticiones desde el origen del cliente React.
* Identifica la lógica útil remanente en `RealEstateApp.Presentation.WebApp` (vistas `.cshtml`, controladores MVC antiguos) y trasládala en forma de Endpoints REST o lógica de Aplicación si hace falta para el frontend de React.

### 2. Validación de Contratos y Reglas de Negocio en el Core (.NET 10)
Cruza las características operativas del frontend React con los casos de uso (`Commands`/`Queries`) en `RealEstateApp.Core.Application` y las entidades en `RealEstateApp.Core.Domain` para garantizar que se cumplen las siguientes restricciones:
* **Regla de Ofertas Recibidas:** Validar que la aceptación de una oferta sea atómica y dispare el rechazo en cascada automático de las demás ofertas del inmueble.
* **Límite de Propietario Directo:** Validar estrictamente en el comando de creación que un usuario con rol de propietario no exceda el máximo de 2 propiedades simultáneas activas.
* **Buscador de 6 Dígitos:** Garantizar que exista el Query optimizado para buscar inmuebles por su código único de 6 caracteres.
* **Soporte Multi-Moneda:** Asegurar que los endpoints financieros utilicen correctamente la tasa de cambio en vivo con el sistema de caché implementado (`IMemoryCache`).

### 3. Implementación de Funcionalidades Pendientes (Próxima Fase)
Modifica el código de la Web API y las capas del Core pertinentes para dejar listos los endpoints y lógicas de backend que alimentarán el frontend en las siguientes características clave:
* **F-01 (AVM):** Endpoint de Valuación Automatizada que estime el precio por m² basado en comparables de la misma provincia/sector.
* **F-04 (CRM Kanban):** Estructura de datos y endpoints para actualizar el estado de arrastre (*Pipeline*) de un Lead (`Nuevo Lead` ➔ `Contactado` ➔ `Visita` ➔ `Oferta` ➔ `Cierre`).
* **F-09 (BuyAbility):** Lógica del backend para evaluar capacidad de compra según ingresos y deudas ingresadas.

---

## 🎯 PASO 3: Verificación y Reporte de Progreso

1. Revisa que no queden dependencias rotas hacia Razor Views o librerías de renderizado del lado del servidor en las capas Core o Web API.
2. Ejecuta los tests unitarios (`xUnit` / `Moq`) del backend y confirma que la suite de pruebas siga en estado exitoso (25/25 aprobados).
3. Preséntame un resumen compacto de los archivos modificados, los eliminados y el estado final de la reestructuración.

**Comienza el análisis del repositorio ahora mismo.**
