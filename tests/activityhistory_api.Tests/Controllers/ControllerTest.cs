using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using activityhistory_api.Controllers;
using activityhistory_application.Commands.Commands;
using activityhistory_application.DTOs;
using activityhistory_application.Queries.Queries;
using activityhistory_application.DTOs.DTOResponse;
using System.Collections.Generic;

namespace activityhistory_api.Tests
{
    public class ActivityHistoryControllersTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ActivityHistoryControllers _controller;

        public ActivityHistoryControllersTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ActivityHistoryControllers(_mediatorMock.Object);
        }

        [Fact]
        public async Task CreateActivity_ShouldReturnOk_WhenRequestIsValid()
        {
            var email = "test@example.com";
            var requestDto = new CreateActivityDTO
            {
                Action = "Login",
                Category = "Auth"
            };

            var expectedResponse = new CreateActivityResponseDto("Login", "2023-01-01", email, "Auth");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateActivityCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.CreateActivity(email, requestDto, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateActivityCommand>(c => c.UserEmail == email && c.ActivityDTO == requestDto),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetHistoryByUserEmail_ShouldReturnOk_WhenCalled()
        {
            var email = "test@example.com";

            var expectedHistory = new List<GetActivityGetByUserEmailResponseDTO>
            {
                new GetActivityGetByUserEmailResponseDTO
                {
                    Action = "Login",
                    TimeDate = "2023-01-01",
                    Category = "Auth",
                },
                new GetActivityGetByUserEmailResponseDTO
                {
                    Action = "Logout",
                    TimeDate = "2023-01-02",
                    Category = "Auth",
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetActivitiesByUserEmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHistory);

            var result = await _controller.GetHistoryByUserEmail(email, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expectedHistory, okResult.Value);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetActivitiesByUserEmailQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}