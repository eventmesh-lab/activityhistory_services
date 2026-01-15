using activityhistory_application.Commands.Commands;
using activityhistory_application.DTOs.DTOResponse;
using activityhistory_domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_application.Mappers
{
    public class ActivityHistoryMappers
    {
        public static ActivityHistory ToDomain(CreateActivityCommand request)
        {

            return new ActivityHistory(request.ActivityDTO.Action, request.TimeDate, request.UserEmail, request.ActivityDTO.Category);
        }

        public static CreateActivityResponseDto ToDto(ActivityHistory activityHistory)
        {

            return new CreateActivityResponseDto(activityHistory.Action, activityHistory.TimeDate.ToString(),activityHistory.UserEmail, activityHistory.Category);
        }
        public static List<GetActivityGetByUserEmailResponseDTO> ToResponseGetList(List<ActivityHistory> users)
        {
            return users.Select(u => new GetActivityGetByUserEmailResponseDTO
            {
                Category= u.Category,
                Action = u.Action,
                TimeDate= u.TimeDate.ToString(),
            }).ToList();
        }

    }
}
