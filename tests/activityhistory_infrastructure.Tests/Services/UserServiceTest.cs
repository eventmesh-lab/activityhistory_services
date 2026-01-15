using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using activityhistory_infrastructure.Services;
using System;

namespace activityhistory_infrastructure.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:7181/")
            };
            _userService = new UserService(_httpClient);
        }

        [Fact]
        public async Task GetIdUserByEmailServices_ShouldReturnGuid_WhenResponseIsSuccessAndValidGuid()
        {
            var email = "test@example.com";
            var expectedGuid = Guid.NewGuid();
            var jsonResponse = $"\"{expectedGuid}\"";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var result = await _userService.GetIdUserByEmailServices(email);

            Assert.Equal(expectedGuid, result);
        }

        [Fact]
        public async Task GetIdUserByEmailServices_ShouldReturnEmptyGuid_WhenResponseIsNotSuccess()
        {
            var email = "test@example.com";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            var result = await _userService.GetIdUserByEmailServices(email);

            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public async Task GetIdUserByEmailServices_ShouldReturnEmptyGuid_WhenResponseIsInvalidGuid()
        {
            var email = "test@example.com";
            var invalidContent = "\"invalid-guid-string\"";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidContent)
                });

            var result = await _userService.GetIdUserByEmailServices(email);

            Assert.Equal(Guid.Empty, result);
        }
    }
}