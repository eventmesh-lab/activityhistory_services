using activityhistory_domain.Entities;
using activityhistory_infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_infrastructure.Mappers
{
    public class ActivityHistoryMappers
    {
        public static ActivityHistory ToDomain(ActivityHistoryPostgres model)
        {
            return new ActivityHistory(model.Id, model.Action, DateTime.Parse(model.TimeDate),model.UserEmail, model.Category);
        }

        public static ActivityHistoryPostgres ToModel(ActivityHistory activity)
        {
            return new ActivityHistoryPostgres
            {
                Id = activity.Id,
                Action = activity.Action,
                TimeDate = activity.TimeDate.ToString(),
                UserEmail = activity.UserEmail,
                Category= activity.Category
            };
        }
    }
}
