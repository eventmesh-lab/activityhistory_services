using activityhistory_application.DTOs.DTOResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_application.Queries.Queries
{
    public class GetActivitiesByUserEmailQuery: IRequest<List<GetActivityGetByUserEmailResponseDTO>>
    {
        public string Email { get; set; }

        public GetActivitiesByUserEmailQuery(string email)
        {
            Email = email;
        }
    }
}
