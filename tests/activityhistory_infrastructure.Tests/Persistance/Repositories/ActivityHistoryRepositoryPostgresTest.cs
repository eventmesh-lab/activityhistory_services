using Xunit;
using Microsoft.EntityFrameworkCore;
using activityhistory_infrastructure.Persistence.Repositories;
using activityhistory_infrastructure.Persistence.Context;
using activityhistory_domain.Entities;
using activityhistory_infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace activityhistory_infrastructure.Tests.Repositories
{
    public class ActivityHistoryRepositoryPostgresTests
    {
        private AppDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddActivity_ShouldAddActivityToDatabase()
        {
            var dbName = Guid.NewGuid().ToString();
            var activity = new ActivityHistory
            {
                Action = "Login",
                UserEmail = "test@example.com",
                Category = "Auth",
                TimeDate = DateTime.UtcNow
            };

            using (var context = GetDbContext(dbName))
            {
                var repository = new ActivityHistoryRepositoryPostgres(context);
                await repository.AddActivity(activity, CancellationToken.None);
            }

            using (var context = GetDbContext(dbName))
            {
                var savedActivity = await context.ActivityHistories.FirstOrDefaultAsync();
                Assert.NotNull(savedActivity);
                Assert.Equal(activity.Action, savedActivity.Action);
                Assert.Equal(activity.UserEmail, savedActivity.UserEmail);
            }
        }

        [Fact]
        public async Task GetActivityByUserEmailAsync_ShouldReturnActivities_WhenEmailMatches()
        {
            var dbName = Guid.NewGuid().ToString();
            var email = "user@example.com";

            using (var context = GetDbContext(dbName))
            {
                context.ActivityHistories.Add(new ActivityHistoryPostgres { Id = Guid.NewGuid(), UserEmail = email, Action = "Login", TimeDate = "2026-01-15T10:00:00Z", Category = "Auth" });
                context.ActivityHistories.Add(new ActivityHistoryPostgres { Id = Guid.NewGuid(), UserEmail = email, Action = "Logout", TimeDate = "2026-01-15T12:00:00Z", Category = "Auth" });
                context.ActivityHistories.Add(new ActivityHistoryPostgres { Id = Guid.NewGuid(), UserEmail = "other@example.com", Action = "Login", TimeDate = "2026-01-15T10:00:00Z", Category = "Auth" });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repository = new ActivityHistoryRepositoryPostgres(context);
                var result = await repository.GetActivityByUserEmailAsync(email);

                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.All(result, r => Assert.Equal(email, r.UserEmail));
            }
        }

        [Fact]
        public async Task GetActivityByUserEmailAsync_ShouldReturnEmptyList_WhenEmailDoesNotMatch()
        {
            var dbName = Guid.NewGuid().ToString();

            using (var context = GetDbContext(dbName))
            {
                context.ActivityHistories.Add(new ActivityHistoryPostgres { Id = Guid.NewGuid(), UserEmail = "other@example.com", Action = "Login", TimeDate = "2026-01-15T10:00:00Z", Category = "Auth" }); await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repository = new ActivityHistoryRepositoryPostgres(context);
                var result = await repository.GetActivityByUserEmailAsync("missing@example.com");

                Assert.NotNull(result);
                Assert.Empty(result);
            }
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityHistoryRepositoryPostgres(null));
        }
    }
}