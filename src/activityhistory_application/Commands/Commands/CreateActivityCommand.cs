using activityhistory_application.DTOs;
using activityhistory_application.DTOs.DTOResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_application.Commands.Commands
{
    public class CreateActivityCommand: IRequest<CreateActivityResponseDto>
    {
        public DateTime TimeDate { get; set; }
        public CreateActivityDTO ActivityDTO { get; set; }
        public string UserEmail { get; set; }
        public CreateActivityCommand(CreateActivityDTO activityDTO, string userEmail)
        {
            ActivityDTO = activityDTO;
            TimeDate = DateTime.Now;
            UserEmail = userEmail;
        }
    }
}
