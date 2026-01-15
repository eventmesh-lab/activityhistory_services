using Xunit;
using Moq;
using Aplicaactivityhistory_applicationtion.Queries.Handlers;
using activityhistory_application.Queries.Queries;
using activityhistory_application.Interfaces;
using activityhistory_domain.Interfaces;
using activityhistory_domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace activityhistory_application.Tests.Handlers
{
    public class GetActivitiesByUserEmailHandlerTests
    {
        private readonly Mock<IActivityHistoryRepositoryPostgres> _activityHistoryRepositoryMock;
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly GetActivitiesByUserEmailHandler _handler;

        public GetActivitiesByUserEmailHandlerTests()
        {
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepositoryPostgres>();
            _userServicesMock = new Mock<IUserServices>();
            _handler = new GetActivitiesByUserEmailHandler(_activityHistoryRepositoryMock.Object, _userServicesMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnActivities_WhenUserExists()
        {
            var email = "test@example.com";
            var query = new GetActivitiesByUserEmailQuery(email);
            var userId = Guid.NewGuid();

            var activities = new List<ActivityHistory>
            {
                new ActivityHistory { Action = "Login", TimeDate = DateTime.UtcNow, UserEmail = email },
                new ActivityHistory { Action = "View", TimeDate = DateTime.UtcNow, UserEmail = email }
            };

            _userServicesMock
                .Setup(x => x.GetIdUserByEmailServices(email))
                .ReturnsAsync(userId);

            _activityHistoryRepositoryMock
                .Setup(x => x.GetActivityByUserEmailAsync(email))
                .ReturnsAsync(activities);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(activities.Count, result.Count);

            _userServicesMock.Verify(x => x.GetIdUserByEmailServices(email), Times.Once);
            _activityHistoryRepositoryMock.Verify(x => x.GetActivityByUserEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenUserDoesNotExist()
        {
            var email = "nonexistent@example.com";
            var query = new GetActivitiesByUserEmailQuery(email);

            _userServicesMock
                .Setup(x => x.GetIdUserByEmailServices(email))
                .ReturnsAsync(Guid.Empty);

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(query, CancellationToken.None));

            Assert.Equal($"No existen el usuario con email {email} en la base de datos.", exception.Message);

            _userServicesMock.Verify(x => x.GetIdUserByEmailServices(email), Times.Once);
            _activityHistoryRepositoryMock.Verify(x => x.GetActivityByUserEmailAsync(It.IsAny<string>()), Times.Never);
        }
    }
}