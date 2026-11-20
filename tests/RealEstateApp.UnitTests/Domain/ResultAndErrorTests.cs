using System;
using FluentAssertions;
using RealEstateApp.Core.Domain.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Domain
{
    public class ResultAndErrorTests
    {
        [Fact]
        public void Success_DebeRetornarResultadoExitosoConErrorNone_CuandoSeInvoca()
        {
            // Arrange & Act
            var result = Result.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().Be(Error.None);
        }

        [Fact]
        public void Failure_DebeRetornarResultadoFallidoConError_CuandoErrorEsValido()
        {
            // Arrange
            var error = Error.Validation("Offer.InvalidAmount", "El monto ofertado es inválido.");

            // Act
            var result = Result.Failure(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void Failure_DebeLanzarInvalidOperationException_CuandoErrorEsNone()
        {
            // Arrange & Act
            Action act = () => Result.Failure(Error.None);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void FailureGeneric_DebeLanzarInvalidOperationException_CuandoErrorEsNone()
        {
            // Arrange & Act
            Action act = () => Result.Failure<string>(Error.None);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SuccessGeneric_DebeRetornarValor_CuandoResultadoEsExitoso()
        {
            // Arrange & Act
            var result = Result.Success<int>(42);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().Be(Error.None);
            result.Value.Should().Be(42);
        }

        [Fact]
        public void Value_DebeLanzarInvalidOperationException_CuandoResultadoEsFallido()
        {
            // Arrange
            var error = Error.NotFound("Property.NotFound", "La propiedad no fue encontrada.");
            var result = Result.Failure<string>(error);

            // Act
            Action act = () => _ = result.Value;

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(error);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ErrorNone_DebeTenerCodigoYMensajeVaciosConTipoFailure_PorDefecto()
        {
            // Arrange & Act
            var none = Error.None;

            // Assert
            none.Code.Should().Be(string.Empty);
            none.Message.Should().Be(string.Empty);
            none.Type.Should().Be(ErrorType.Failure);
        }

        [Theory]
        [InlineData(ErrorType.Validation, "Val.Code", "Error de validación")]
        [InlineData(ErrorType.NotFound, "NotFound.Code", "Recurso no encontrado")]
        [InlineData(ErrorType.Conflict, "Conflict.Code", "Conflicto de estado")]
        [InlineData(ErrorType.Forbidden, "Forbidden.Code", "Acceso prohibido")]
        [InlineData(ErrorType.Unauthorized, "Unauthorized.Code", "No autorizado")]
        [InlineData(ErrorType.Failure, "Failure.Code", "Fallo general")]
        public void ErrorFactories_DebeAsignarCodeMessageYTypeExactos_SegunFabrica(
            ErrorType expectedType,
            string code,
            string message)
        {
            // Arrange & Act
            var error = expectedType switch
            {
                ErrorType.Validation => Error.Validation(code, message),
                ErrorType.NotFound => Error.NotFound(code, message),
                ErrorType.Conflict => Error.Conflict(code, message),
                ErrorType.Forbidden => Error.Forbidden(code, message),
                ErrorType.Unauthorized => Error.Unauthorized(code, message),
                ErrorType.Failure => Error.Failure(code, message),
                _ => throw new ArgumentOutOfRangeException(nameof(expectedType))
            };

            // Assert
            error.Code.Should().Be(code);
            error.Message.Should().Be(message);
            error.Type.Should().Be(expectedType);
        }

        [Fact]
        public void ImplicitOperatorResult_DebeConvertirErrorAResultFallido_CuandoSeAsignaError()
        {
            // Arrange
            var error = Error.Conflict("Offer.AlreadyProcessed", "La oferta ya fue procesada.");

            // Act
            Result result = error;

            // Assert
            result.IsFailure.Should().BeTrue();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void ImplicitOperatorResultGeneric_DebeConvertirValorAResultExitoso_CuandoSeAsignaValor()
        {
            // Arrange & Act
            Result<string> ok = "hola";

            // Assert
            ok.IsSuccess.Should().BeTrue();
            ok.IsFailure.Should().BeFalse();
            ok.Error.Should().Be(Error.None);
            ok.Value.Should().Be("hola");
        }

        [Fact]
        public void ImplicitOperatorResultGeneric_DebeConvertirErrorAResultFallido_CuandoSeAsignaError()
        {
            // Arrange
            var error = Error.Forbidden("Property.Forbidden", "No tiene permisos.");

            // Act
            Result<string> fail = error;
            Action act = () => _ = fail.Value;

            // Assert
            fail.IsFailure.Should().BeTrue();
            fail.IsSuccess.Should().BeFalse();
            fail.Error.Should().Be(error);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ErrorEquality_DebeCompararEstructuralmentePorValor_CuandoSonRecords()
        {
            // Arrange
            var first = Error.Unauthorized("Auth.Invalid", "Credenciales inválidas");
            var second = Error.Unauthorized("Auth.Invalid", "Credenciales inválidas");
            var differentType = Error.Forbidden("Auth.Invalid", "Credenciales inválidas");

            // Act & Assert
            first.Should().Be(second);
            (first == second).Should().BeTrue();
            (first == differentType).Should().BeFalse();
        }
    }
}
