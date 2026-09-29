# Estándares de Arquitectura Limpia y Fronteras (Clean Architecture)

## 1. Principio Fundamental de Fronteras Limpias (Uncle Bob)
Las dependencias en el código fuente solo pueden apuntar **hacia adentro**, en dirección a las políticas de más alto nivel:
`Presentation` ➔ `Infrastructure` ➔ `Core.Application` ➔ `Core.Domain`.

Ningún elemento de una capa interna debe conocer detalles ni nomenclaturas de una capa externa.

---

## 2. Regla de DTOs vs. ViewModels (Solución al Pecado Capital #6)

### ❌ Práctica Prohibida
- **NO crear nuevos archivos en `src/Core/RealEstateApp.Core.Application/ViewModels/`**.
- La palabra *ViewModel* pertenece ontológicamente a la capa de **Presentación** (el modelo específico que una vista o componente visual consume).
- La capa `Core.Application` no debe hablar el lenguaje de las pantallas, sino el lenguaje de los **Casos de Uso del Negocio**.

### ✅ Práctica Obligatoria
1. **Modelos de Entrada (Comandos/Queries)**:
   - Ubicación: `src/Core/RealEstateApp.Core.Application/DTOs/{Modulo}/`
   - Nomenclatura: `[Accion][Entidad]Request` (ej. `CreatePropertyRequest`, `UpdateOfferRequest`).
2. **Modelos de Salida (Resultados de Casos de Uso)**:
   - Ubicación: `src/Core/RealEstateApp.Core.Application/DTOs/{Modulo}/`
   - Nomenclatura: `[Entidad]Response` o `[Modulo]Dto` (ej. `PropertyResponse`, `DashboardMetricsDto`, `AccountUserDto`).
3. **ViewModels Existentes**:
   - Se mantienen temporalmente por retrocompatibilidad binaria con controladores heredados.
   - Cualquier nueva funcionalidad (Feature V2+) debe crearse utilizando DTOs `Request`/`Response`.

---

## 3. Regla Anti-Mockitis en Pruebas Unitarias (Solución al Pecado Capital #5)

### ❌ Anti-Patrón (Mockitis)
- Crear mocks de repositorios solo para verificar `repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<T>()), Times.Once)`.
- Esto prueba la implementación privada del método (acoplamiento estructural), no si el estado del sistema cambió correctamente. Si se cambia la implementación, el test falla aunque el negocio funcione.

### ✅ Práctica Obligatoria (Social State-Based Testing)
- Para pruebas de servicios que mutan datos relacionales o reglas complejas (ej. `OfferService`, `PropertyService`):
  - Utilizar un `ApplicationDbContext` configurado con `UseInMemoryDatabase(Guid.NewGuid().ToString())`.
  - Instanciar los repositorios reales de persistencia y el `UnitOfWork`.
  - Ejecutar el caso de uso y **asertar sobre el estado real de las entidades persistidas en la BD** (ej. `entityInDb.Status.Should().Be(...)`).
  - Reservar `Mock<T>` únicamente para dependencias externas incontrolables (ej. envío de emails, subida de archivos físicos a disco o APIs de terceros).

---

## 4. Consistencia Transaccional y Unit of Work (Pecado Capital #4)
- Toda operación multi-repositorio que modifique más de una tabla o entidad debe ejecutarse bajo `IUnitOfWork.ExecuteTransactionAsync(...)`.
- Los repositorios no deben ejecutar transacciones independientes que rompan la atomicidad orquestada por el caso de uso.
