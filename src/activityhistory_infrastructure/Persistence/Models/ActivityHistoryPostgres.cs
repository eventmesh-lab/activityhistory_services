using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_infrastructure.Persistence.Models
{
    public class ActivityHistoryPostgres
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public string Action { get; set; }
        public string TimeDate { get; set; }
        public string UserEmail { get; set; }

    }
}
