using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_application.DTOs.DTOResponse
{
    public class CreateActivityResponseDto
    {
        public string Category { get; set; }
        public string Action { get; set; }
        public string TimeDate { get; set; }
        public string UserEmail { get; set; }

        public CreateActivityResponseDto(string action, string timeDate, string userEmail, string category)
        {
            Action = action;
            TimeDate = timeDate;
            UserEmail = userEmail;
            Category = category;
        }
    }
}
