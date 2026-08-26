# Norma de Calidad y Pruebas Unitarias Continuas

## 1. Política Mandatoria
Toda nueva característica, mejora funcional (ej. nuevas Features F-03, F-04, etc.), refactorización o corrección de defectos en `RealEstateApp` **debe incluir obligatoriamente** sus respectivas pruebas unitarias antes de darse por completada.

## 2. Ubicación y Nomenclatura
- **Proyecto de pruebas**: `tests/RealEstateApp.UnitTests/RealEstateApp.UnitTests.csproj`
- **Ubicación de archivos**:
  - `tests/RealEstateApp.UnitTests/Services/{ServiceName}Tests.cs` para servicios de aplicación.
  - `tests/RealEstateApp.UnitTests/Controllers/{ControllerName}Tests.cs` para controladores API o WebApp.
  - `tests/RealEstateApp.UnitTests/Domain/{EntityName}Tests.cs` para entidades y reglas puras.
- **Nomenclatura de Métodos de Test**:
  `[MetodoOAccion]_[ComportamientoEsperado]_[BajoQueCondicion]`
  - Ejemplo: `Add_ShouldRecordInitialPriceHistory_WhenPropertyIsCreated`
  - Ejemplo: `Update_ShouldRecordPriceDropHistory_WhenPriceDecreases`

## 3. Estándar AAA (Arrange - Act - Assert)
Cada prueba unitaria debe estar claramente segmentada en las tres fases del patrón AAA:
```csharp
[Fact]
public async Task MiMetodo_DebeHacerX_CuandoCondicionY()
{
    // Arrange (Preparación de datos, mocks y dependencias)
    ...

    // Act (Ejecución del método o acción bajo prueba)
    ...

    // Assert (Aserciones usando FluentAssertions)
    ...
}
```

## 4. Stack y Herramientas
- **Runner / Framework**: `xUnit`
- **Mocking**: `Moq` (para repositorios, servicios externos, HTTP clients, caché)
- **Aserciones**: `FluentAssertions`
- **Mapeos**: `AutoMapperTestFactory.CreateMapper()`

## 5. Criterio de Aceptación
Ningún cambio se considera listo hasta que la ejecución de `dotnet test` reporte **0 errores y 100% de tests exitosos**.
