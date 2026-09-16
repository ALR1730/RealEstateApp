using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Appointment;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
        private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IUserActivityService> _userActivityServiceMock;
        private readonly IMapper _mapper;
        private readonly AppointmentService _sut;

        public AppointmentServiceTests()
        {
            _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _accountServiceMock = new Mock<IAccountService>();
            _userActivityServiceMock = new Mock<IUserActivityService>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _sut = new AppointmentService(
                _appointmentRepositoryMock.Object,
                _propertyRepositoryMock.Object,
                _accountServiceMock.Object,
                _userActivityServiceMock.Object,
                _mapper
            );
        }

        [Fact]
        public async Task RequestAppointmentAsync_ShouldCreatePendingAppointment_WhenDateIsInFuture()
        {
            // Arrange
            int propertyId = 3;
            var property = new Property
            {
                Id = propertyId,
                Name = "Torre Corporativa",
                Code = "EDI001",
                AgentId = "agent-999"
            };

            var futureDate = DateTime.Now.AddDays(3);
            var vm = new SaveAppointmentViewModel
            {
                PropertyId = propertyId,
                AppointmentDate = futureDate,
                Comments = "Visita con inversionista"
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            _appointmentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PropertyAppointment>()))
                .ReturnsAsync((PropertyAppointment a) => { a.Id = 50; return a; });

            _accountServiceMock.Setup(a => a.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new AccountUserDto { FirstName = "Juan", LastName = "Perez", Email = "juan@test.com" });

            // Act
            var result = await _sut.RequestAppointmentAsync(vm, "client-100");

            // Assert
            result.Should().NotBeNull();
            _appointmentRepositoryMock.Verify(r => r.AddAsync(It.Is<PropertyAppointment>(a => 
                a.PropertyId == propertyId && 
                a.AgentId == "agent-999" && 
                a.ClienteId == "client-100" && 
                a.Status == AppointmentStatus.Pending
            )), Times.Once);
        }

        [Fact]
        public async Task RequestAppointmentAsync_ShouldThrowDomainException_WhenDateIsInPast()
        {
            // Arrange
            int propertyId = 3;
            var property = new Property
            {
                Id = propertyId,
                Name = "Torre Corporativa",
                AgentId = "agent-999"
            };

            var pastDate = DateTime.Now.AddDays(-2);
            var vm = new SaveAppointmentViewModel
            {
                PropertyId = propertyId,
                AppointmentDate = pastDate
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            // Act
            Func<Task> act = async () => await _sut.RequestAppointmentAsync(vm, "client-100");

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("La fecha propuesta para la cita debe ser en el futuro.");
        }

        [Fact]
        public async Task ConfirmAppointmentAsync_ShouldUpdateStatusToConfirmed()
        {
            // Arrange
            int appointmentId = 25;
            string agentId = "agent-007";
            var appointment = new PropertyAppointment
            {
                Id = appointmentId,
                AgentId = agentId,
                ClienteId = "client-001",
                PropertyId = 1,
                Status = AppointmentStatus.Pending
            };

            _appointmentRepositoryMock.Setup(r => r.GetByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            // Act
            await _sut.ConfirmAppointmentAsync(appointmentId, agentId, "Confirmado. Nos vemos en el lobby.");

            // Assert
            appointment.Status.Should().Be(AppointmentStatus.Confirmed);
            appointment.AgentNotes.Should().Be("Confirmado. Nos vemos en el lobby.");
            _appointmentRepositoryMock.Verify(r => r.UpdateAsync(appointment), Times.Once);
        }
    }
}
