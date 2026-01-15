using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_domain.Entities
{
    public class ActivityHistory
    {
        public Guid Id { get; set; } 
        public string Category { get; set; }
        public string Action { get; set; }
        public DateTime TimeDate { get; set; }
        public string UserEmail { get; set; }

        public ActivityHistory(string action, DateTime timeDate, string userEmail, string category)
        {
            Id = Guid.NewGuid();
            Action = action;
            TimeDate = timeDate;
            UserEmail = userEmail;
            Category = category;
        }
        public ActivityHistory(Guid id,string action, DateTime timeDate, string userEmail,string category)
        {
            Id = id;
            Action = action;
            TimeDate = timeDate;
            UserEmail = userEmail;
            Category = category;
        }
        public ActivityHistory()
        {
        }
    }
}
