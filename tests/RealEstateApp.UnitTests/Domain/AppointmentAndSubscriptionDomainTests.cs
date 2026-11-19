using System;
using FluentAssertions;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Core.Domain.ValueObjects;
using Xunit;

namespace RealEstateApp.UnitTests.Domain
{
    public class AppointmentAndSubscriptionDomainTests
    {
        #region PropertyAppointment

        [Fact]
        public void PropertyAppointment_DebeTenerStatusConSetterPrivado()
        {
            // Arrange
            var statusProperty = typeof(PropertyAppointment).GetProperty(nameof(PropertyAppointment.Status));

            // Act & Assert
            statusProperty.Should().NotBeNull();
            statusProperty!.SetMethod.Should().NotBeNull();
            statusProperty.SetMethod!.IsPrivate.Should().BeTrue();
        }

        [Fact]
        public void Schedule_DebeCrearCitaEnEstadoPending_CuandoParametrosSonValidos()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(2);

            // Act
            var appointment = PropertyAppointment.Schedule(
                propertyId: 10,
                clienteId: "client-1",
                agentId: "agent-1",
                appointmentDate: futureDate,
                notes: "Prefiero horario de la tarde");

            // Assert
            appointment.Should().NotBeNull();
            appointment.PropertyId.Should().Be(10);
            appointment.ClienteId.Should().Be("client-1");
            appointment.AgentId.Should().Be("agent-1");
            appointment.AppointmentDate.Should().Be(futureDate);
            appointment.Notes.Should().Be("Prefiero horario de la tarde");
            appointment.Status.Should().Be(AppointmentStatus.Pending);
        }

        [Theory]
        [InlineData(0, "client-1", "agent-1")]
        [InlineData(-5, "client-1", "agent-1")]
        [InlineData(10, "", "agent-1")]
        [InlineData(10, "   ", "agent-1")]
        [InlineData(10, "client-1", "")]
        [InlineData(10, "client-1", "   ")]
        public void Schedule_DebeLanzarDomainException_CuandoIdentificadoresSonInvalidos(
            int propertyId,
            string clienteId,
            string agentId)
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(2);

            // Act
            Action act = () => PropertyAppointment.Schedule(propertyId, clienteId, agentId, futureDate);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Schedule_DebeLanzarDomainException_CuandoFechaEsPasada()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-2);

            // Act
            Action act = () => PropertyAppointment.Schedule(10, "client-1", "agent-1", pastDate);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Confirm_DebeTransicionarAPendingAConfirmed_CuandoAgenteEsPropietarioDeLaCita()
        {
            // Arrange
            var appointment = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));

            // Act
            appointment.Confirm("agent-1", "Confirmada para las 3:00 PM");

            // Assert
            appointment.Status.Should().Be(AppointmentStatus.Confirmed);
            appointment.AgentNotes.Should().Be("Confirmada para las 3:00 PM");
        }

        [Fact]
        public void Confirm_DebeLanzarDomainException_CuandoUsuarioNoEsElAgenteAsignado()
        {
            // Arrange
            var appointment = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));

            // Act
            Action act = () => appointment.Confirm("client-1");

            // Assert
            act.Should().Throw<DomainException>();
            appointment.Status.Should().Be(AppointmentStatus.Pending);
        }

        [Fact]
        public void Confirm_DebeLanzarDomainException_CuandoCitaNoEstaPending()
        {
            // Arrange
            var appointment = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));
            appointment.Confirm("agent-1");

            // Act
            Action act = () => appointment.Confirm("agent-1");

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Theory]
        [InlineData("client-1")]
        [InlineData("agent-1")]
        public void Cancel_DebeCancelarCita_CuandoLaSolicitaClienteOAgente(string userId)
        {
            // Arrange
            var appointment = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));

            // Act
            appointment.Cancel(userId, "Cambio de planes");

            // Assert
            appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        }

        [Fact]
        public void Cancel_DebeLanzarDomainException_CuandoUsuarioNoParticipaEnLaCita()
        {
            // Arrange
            var appointment = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));

            // Act
            Action act = () => appointment.Cancel("intruder-user", "Intento no autorizado");

            // Assert
            act.Should().Throw<DomainException>();
            appointment.Status.Should().Be(AppointmentStatus.Pending);
        }

        [Fact]
        public void Cancel_DebeLanzarDomainException_CuandoCitaYaFueCompletadaOCancelada()
        {
            // Arrange
            var completedAppointment = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));
            completedAppointment.Complete("agent-1");

            var cancelledAppointment = PropertyAppointment.Schedule(11, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));
            cancelledAppointment.Cancel("client-1");

            // Act
            Action actCompleted = () => completedAppointment.Cancel("client-1");
            Action actCancelled = () => cancelledAppointment.Cancel("agent-1");

            // Assert
            actCompleted.Should().Throw<DomainException>();
            actCancelled.Should().Throw<DomainException>();
        }

        [Fact]
        public void Complete_DebeCompletarCita_DesdePendingYDesdeConfirmedParaElAgente()
        {
            // Arrange
            var fromPending = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));
            var fromConfirmed = PropertyAppointment.Schedule(11, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));
            fromConfirmed.Confirm("agent-1");

            // Act
            fromPending.Complete("agent-1", "Visita realizada");
            fromConfirmed.Complete("agent-1", "Cliente interesado");

            // Assert
            fromPending.Status.Should().Be(AppointmentStatus.Completed);
            fromPending.AgentNotes.Should().Be("Visita realizada");
            fromConfirmed.Status.Should().Be(AppointmentStatus.Completed);
            fromConfirmed.AgentNotes.Should().Be("Cliente interesado");
        }

        [Fact]
        public void Complete_DebeLanzarDomainException_CuandoCitaEstaCanceladaOQuienCompletaNoEsElAgente()
        {
            // Arrange
            var cancelledAppointment = PropertyAppointment.Schedule(10, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));
            cancelledAppointment.Cancel("client-1");

            var pendingAppointment = PropertyAppointment.Schedule(11, "client-1", "agent-1", DateTime.UtcNow.AddDays(2));

            // Act
            Action actFromCancelled = () => cancelledAppointment.Complete("agent-1");
            Action actWrongActor = () => pendingAppointment.Complete("client-1");

            // Assert
            actFromCancelled.Should().Throw<DomainException>();
            actWrongActor.Should().Throw<DomainException>();
        }

        #endregion

        #region AgentSubscription

        [Fact]
        public void AgentSubscription_DebeTenerIsActiveEndDateYAutoRenewConSetterPrivado()
        {
            // Arrange
            var isActiveProp = typeof(AgentSubscription).GetProperty(nameof(AgentSubscription.IsActive));
            var endDateProp = typeof(AgentSubscription).GetProperty(nameof(AgentSubscription.EndDate));
            var autoRenewProp = typeof(AgentSubscription).GetProperty(nameof(AgentSubscription.AutoRenew));

            // Act & Assert
            isActiveProp.Should().NotBeNull();
            isActiveProp!.SetMethod.Should().NotBeNull();
            isActiveProp.SetMethod!.IsPrivate.Should().BeTrue();

            endDateProp.Should().NotBeNull();
            endDateProp!.SetMethod.Should().NotBeNull();
            endDateProp.SetMethod!.IsPrivate.Should().BeTrue();

            autoRenewProp.Should().NotBeNull();
            autoRenewProp!.SetMethod.Should().NotBeNull();
            autoRenewProp.SetMethod!.IsPrivate.Should().BeTrue();
        }

        [Fact]
        public void CreateActive_DebeCrearSuscripcionActivaConFechaFinFutura_CuandoParametrosSonValidos()
        {
            // Arrange & Act
            var subscription = AgentSubscription.CreateActive(
                agentId: "agent-1",
                subscriptionPlanId: 2,
                durationMonths: 3,
                autoRenew: false);

            // Assert
            subscription.Should().NotBeNull();
            subscription.AgentId.Should().Be("agent-1");
            subscription.SubscriptionPlanId.Should().Be(2);
            subscription.IsActive.Should().BeTrue();
            subscription.AutoRenew.Should().BeFalse();
            subscription.EndDate.Should().NotBeNull();
            subscription.EndDate!.Value.Should().BeAfter(subscription.StartDate);
        }

        [Theory]
        [InlineData("", 1, 1)]
        [InlineData("   ", 1, 1)]
        [InlineData("agent-1", 0, 1)]
        [InlineData("agent-1", -2, 1)]
        [InlineData("agent-1", 1, 0)]
        [InlineData("agent-1", 1, -3)]
        public void CreateActive_DebeLanzarDomainException_CuandoParametrosSonInvalidos(
            string agentId,
            int subscriptionPlanId,
            int durationMonths)
        {
            // Arrange & Act
            Action act = () => AgentSubscription.CreateActive(agentId, subscriptionPlanId, durationMonths);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Cancel_DebeDesactivarSuscripcionDeshabilitarAutoRenewYFijarEndDate()
        {
            // Arrange
            var subscription = AgentSubscription.CreateActive("agent-1", 1, durationMonths: 1, autoRenew: true);
            var cancelledAt = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

            // Act
            subscription.Cancel(cancelledAt);

            // Assert
            subscription.IsActive.Should().BeFalse();
            subscription.AutoRenew.Should().BeFalse();
            subscription.EndDate.Should().Be(cancelledAt);
        }

        [Fact]
        public void Cancel_DebeLanzarDomainException_CuandoSuscripcionYaEstaInactiva()
        {
            // Arrange
            var subscription = AgentSubscription.CreateActive("agent-1", 1);
            subscription.Cancel();

            // Act
            Action act = () => subscription.Cancel();

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Renew_DebeExtenderEndDateYActivarSuscripcion_CuandoMesesAdicionalesSonValidos()
        {
            // Arrange
            var subscription = AgentSubscription.CreateActive("agent-1", 1, durationMonths: 1);
            var originalEndDate = subscription.EndDate!.Value;

            // Act
            subscription.Renew(additionalMonths: 2);

            // Assert
            subscription.IsActive.Should().BeTrue();
            subscription.EndDate.Should().NotBeNull();
            subscription.EndDate!.Value.Should().BeAfter(originalEndDate);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Renew_DebeLanzarDomainException_CuandoMesesAdicionalesNoSonPositivos(int invalidMonths)
        {
            // Arrange
            var subscription = AgentSubscription.CreateActive("agent-1", 1);

            // Act
            Action act = () => subscription.Renew(invalidMonths);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void IsExpired_DebeRetornarFalseAntesDeEndDateYTrueDespuesDeEndDate()
        {
            // Arrange
            var subscription = AgentSubscription.CreateActive("agent-1", 1, durationMonths: 1);
            var endDate = subscription.EndDate!.Value;

            // Act
            var beforeEnd = subscription.IsExpired(endDate.AddMinutes(-1));
            var afterEnd = subscription.IsExpired(endDate.AddMinutes(1));

            // Assert
            beforeEnd.Should().BeFalse();
            afterEnd.Should().BeTrue();
        }

        #endregion

        #region Offer

        [Fact]
        public void Offer_DebeTenerStatusConSetterPrivado()
        {
            // Arrange
            var statusProperty = typeof(Offer).GetProperty(nameof(Offer.Status));

            // Act & Assert
            statusProperty.Should().NotBeNull();
            statusProperty!.SetMethod.Should().NotBeNull();
            statusProperty.SetMethod!.IsPrivate.Should().BeTrue();
        }

        [Fact]
        public void RejectCounterOffer_DebeRechazarContraOferta_CuandoClienteEsElAutorYEstadoEsCounterOffered()
        {
            // Arrange
            var offer = new Offer
            {
                PropertyId = 1,
                ClienteId = "client-1",
                MontoOfertado = 4_000_000m
            };
            offer.MakeCounterOffer(4_400_000m, "Podemos cerrar en 4.4M");

            // Act
            offer.RejectCounterOffer("client-1");

            // Assert
            offer.Status.Should().Be(OfferStatus.Rejected);
        }

        [Fact]
        public void RejectCounterOffer_DebeLanzarDomainException_CuandoClienteEsDistintoOEstadoNoEsCounterOffered()
        {
            // Arrange
            var counterOffered = new Offer
            {
                PropertyId = 1,
                ClienteId = "client-1",
                MontoOfertado = 4_000_000m
            };
            counterOffered.MakeCounterOffer(4_400_000m, "Contraoferta");

            var pendingOffer = new Offer
            {
                PropertyId = 1,
                ClienteId = "client-1",
                MontoOfertado = 4_000_000m
            };

            // Act
            Action actWrongClient = () => counterOffered.RejectCounterOffer("client-other");
            Action actNotCounterOffered = () => pendingOffer.RejectCounterOffer("client-1");

            // Assert
            actWrongClient.Should().Throw<DomainException>();
            counterOffered.Status.Should().Be(OfferStatus.CounterOffered);

            actNotCounterOffered.Should().Throw<DomainException>();
            pendingOffer.Status.Should().Be(OfferStatus.Pending);
        }

        [Fact]
        public void RejectOnPropertySold_DebeRechazarOfertasPendingYCounterOffered()
        {
            // Arrange
            var pendingOffer = new Offer { PropertyId = 1, ClienteId = "c1", MontoOfertado = 1000m };
            var counterOffer = new Offer { PropertyId = 1, ClienteId = "c2", MontoOfertado = 1000m };
            counterOffer.MakeCounterOffer(1200m, "Contraoferta");

            // Act
            pendingOffer.RejectOnPropertySold();
            counterOffer.RejectOnPropertySold();

            // Assert
            pendingOffer.Status.Should().Be(OfferStatus.Rejected);
            counterOffer.Status.Should().Be(OfferStatus.Rejected);
        }

        [Fact]
        public void RejectOnPropertySold_DebeLanzarDomainException_CuandoOfertaYaEstaAceptadaORechazada()
        {
            // Arrange
            var acceptedOffer = new Offer { PropertyId = 1, ClienteId = "c1", MontoOfertado = 1000m };
            acceptedOffer.Accept();

            var rejectedOffer = new Offer { PropertyId = 1, ClienteId = "c2", MontoOfertado = 1000m };
            rejectedOffer.Reject();

            // Act
            Action actAccepted = () => acceptedOffer.RejectOnPropertySold();
            Action actRejected = () => rejectedOffer.RejectOnPropertySold();

            // Assert
            actAccepted.Should().Throw<DomainException>();
            actRejected.Should().Throw<DomainException>();
        }

        #endregion

        #region Property

        [Fact]
        public void Property_DebeTenerStatusConSetterPrivado()
        {
            // Arrange
            var statusProperty = typeof(Property).GetProperty(nameof(Property.Status));

            // Act & Assert
            statusProperty.Should().NotBeNull();
            statusProperty!.SetMethod.Should().NotBeNull();
            statusProperty.SetMethod!.IsPrivate.Should().BeTrue();
        }

        [Fact]
        public void AssignCode_DebeAsignarCodigoNormalizado_CuandoPropertyCodeEsValido()
        {
            // Arrange
            var property = new Property { Name = "Apartamento Naco" };
            var code = new PropertyCode("nac123");

            // Act
            property.AssignCode(code);

            // Assert
            property.Code.Should().Be("NAC123");
        }

        [Fact]
        public void AssignToAgent_DebeAsignarAgente_CuandoAgentIdEsValido()
        {
            // Arrange
            var property = new Property { Name = "Casa Evaristo Morales" };

            // Act
            property.AssignToAgent("agent-99");

            // Assert
            property.AgentId.Should().Be("agent-99");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AssignToAgent_DebeLanzarDomainException_CuandoAgentIdEsNuloOVacio(string? invalidAgentId)
        {
            // Arrange
            var property = new Property { Name = "Casa Evaristo Morales", AgentId = "agent-1" };

            // Act
            Action act = () => property.AssignToAgent(invalidAgentId!);

            // Assert
            act.Should().Throw<DomainException>();
            property.AgentId.Should().Be("agent-1");
        }

        #endregion
    }
}
