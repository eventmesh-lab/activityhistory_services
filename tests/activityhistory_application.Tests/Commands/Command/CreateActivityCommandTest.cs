using Xunit;
using activityhistory_application.Commands.Commands;
using activityhistory_application.DTOs;

namespace activityhistory_application.Tests.Commands.Command
{
    public class CreateActivityCommandTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            var activityDto = new CreateActivityDTO
            {
                Action = "Login",
                Category = "Auth"
            };
            var userEmail = "test@example.com";

            var command = new CreateActivityCommand(activityDto, userEmail);

            Assert.Equal(activityDto, command.ActivityDTO);
            Assert.Equal(userEmail, command.UserEmail);
            Assert.NotEqual(default, command.TimeDate);
            Assert.True((DateTime.Now - command.TimeDate).TotalSeconds < 5);
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            var initialDto = new CreateActivityDTO();
            var initialEmail = "initial@example.com";
            var command = new CreateActivityCommand(initialDto, initialEmail);

            var newDate = DateTime.Now.AddHours(1);
            var newEmail = "updated@example.com";
            var newDto = new CreateActivityDTO { Action = "Logout" };

            command.TimeDate = newDate;
            command.UserEmail = newEmail;
            command.ActivityDTO = newDto;

            Assert.Equal(newDate, command.TimeDate);
            Assert.Equal(newEmail, command.UserEmail);
            Assert.Equal(newDto, command.ActivityDTO);
        }
    }
}