using activityhistory_domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_domain.Interfaces
{
    public interface IActivityHistoryRepositoryPostgres
    {
        Task AddActivity(ActivityHistory activity, CancellationToken cancellationToken);
        Task<List<ActivityHistory>> GetActivityByUserEmailAsync(string email);
    }
}
