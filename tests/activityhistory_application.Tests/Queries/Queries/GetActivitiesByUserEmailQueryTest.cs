using Xunit;
using activityhistory_application.Queries.Queries;

namespace activityhistory_application.Tests.Queries
{
    public class GetActivitiesByUserEmailQueryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeEmailCorrectly()
        {
            var email = "test@example.com";

            var query = new GetActivitiesByUserEmailQuery(email);

            Assert.Equal(email, query.Email);
        }

        [Fact]
        public void Email_ShouldBeSettable()
        {
            var initialEmail = "initial@example.com";
            var query = new GetActivitiesByUserEmailQuery(initialEmail);
            var newEmail = "updated@example.com";

            query.Email = newEmail;

            Assert.Equal(newEmail, query.Email);
        }
    }
}