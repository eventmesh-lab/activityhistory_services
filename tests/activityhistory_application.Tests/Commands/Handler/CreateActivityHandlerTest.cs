using Xunit;
using Moq;
using activityhistory_application.Commands.Handlers;
using activityhistory_application.Commands.Commands;
using activityhistory_application.DTOs;
using activityhistory_application.Interfaces;
using activityhistory_domain.Interfaces;
using activityhistory_domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace activityhistory_application.Tests.Handlers
{
    public class CreateActivityHandlerTests
    {
        private readonly Mock<IActivityHistoryRepositoryPostgres> _activityHistoryRepositoryMock;
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly CreateActivityHandler _handler;

        public CreateActivityHandlerTests()
        {
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepositoryPostgres>();
            _userServicesMock = new Mock<IUserServices>();
            _handler = new CreateActivityHandler(_activityHistoryRepositoryMock.Object, _userServicesMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnResponse_WhenUserExistsAndActivityIsAdded()
        {
            var email = "test@example.com";
            var dto = new CreateActivityDTO { Action = "Test Action", Category = "Test Category" };
            var command = new CreateActivityCommand(dto, email);
            var userId = Guid.NewGuid();

            _userServicesMock
                .Setup(x => x.GetIdUserByEmailServices(email))
                .ReturnsAsync(userId);

            _activityHistoryRepositoryMock
                .Setup(x => x.AddActivity(It.IsAny<ActivityHistory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(dto.Action, result.Action);
            Assert.Equal(email, result.UserEmail);

            _userServicesMock.Verify(x => x.GetIdUserByEmailServices(email), Times.Once);
            _activityHistoryRepositoryMock.Verify(x => x.AddActivity(It.IsAny<ActivityHistory>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenUserIdIsEmpty()
        {
            var email = "test@example.com";
            var dto = new CreateActivityDTO { Action = "Test Action" };
            var command = new CreateActivityCommand(dto, email);

            _userServicesMock
                .Setup(x => x.GetIdUserByEmailServices(email))
                .ReturnsAsync(Guid.Empty);

            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));

            _userServicesMock.Verify(x => x.GetIdUserByEmailServices(email), Times.Once);
            _activityHistoryRepositoryMock.Verify(x => x.AddActivity(It.IsAny<ActivityHistory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenRepositoryThrowsException()
        {
            var email = "test@example.com";
            var dto = new CreateActivityDTO { Action = "Test Action" };
            var command = new CreateActivityCommand(dto, email);
            var userId = Guid.NewGuid();

            _userServicesMock
                .Setup(x => x.GetIdUserByEmailServices(email))
                .ReturnsAsync(userId);

            _activityHistoryRepositoryMock
                .Setup(x => x.AddActivity(It.IsAny<ActivityHistory>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("No se pudo registar la actividad en la base de datos", exception.Message);

            _userServicesMock.Verify(x => x.GetIdUserByEmailServices(email), Times.Once);
            _activityHistoryRepositoryMock.Verify(x => x.AddActivity(It.IsAny<ActivityHistory>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}