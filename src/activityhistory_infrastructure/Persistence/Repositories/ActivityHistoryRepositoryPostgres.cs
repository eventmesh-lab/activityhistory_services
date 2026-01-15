using activityhistory_domain.Entities;
using activityhistory_domain.Interfaces;
using activityhistory_infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using activityhistory_infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace activityhistory_infrastructure.Persistence.Repositories
{
    public class ActivityHistoryRepositoryPostgres : IActivityHistoryRepositoryPostgres
    {
        public readonly AppDbContext _context;

        public ActivityHistoryRepositoryPostgres(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddActivity(ActivityHistory activity, CancellationToken cancellationToken)
        {
            var model = ActivityHistoryMappers.ToModel(activity);
            _context.ActivityHistories.Add(model);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<ActivityHistory>> GetActivityByUserEmailAsync(string email)
        {

            var entities = await _context.ActivityHistories
                .Where(a => a.UserEmail == email).ToListAsync();
            var activities = entities.Select(ActivityHistoryMappers.ToDomain).ToList();

            return activities;
        }
    }
}
