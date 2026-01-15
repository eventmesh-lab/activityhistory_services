using activityhistory_application.DTOs.DTOResponse;
using activityhistory_application.Interfaces;
using activityhistory_application.Mappers;
using activityhistory_application.Queries.Queries;
using activityhistory_domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicaactivityhistory_applicationtion.Queries.Handlers
{
    public class GetActivitiesByUserEmailHandler : IRequestHandler<GetActivitiesByUserEmailQuery, List<GetActivityGetByUserEmailResponseDTO>>
    {
        public readonly IMediator _mediator;
        public readonly IUserServices _userServices;
        public readonly IActivityHistoryRepositoryPostgres _activityHistoryServices;
        public GetActivitiesByUserEmailHandler(IActivityHistoryRepositoryPostgres activityHistoryServices, IUserServices userServices)
        {
            _activityHistoryServices = activityHistoryServices;
            _userServices = userServices;
        }
        public async Task<List<GetActivityGetByUserEmailResponseDTO>> Handle(GetActivitiesByUserEmailQuery request, CancellationToken cancellationToken)
        {
            var usersRegistered = await _userServices.GetIdUserByEmailServices(request.Email);
            Console.WriteLine($"Usuario obtenido: {usersRegistered}");
            if (usersRegistered == Guid.Empty)
            {
                throw new ApplicationException($"No existen el usuario con email {request.Email} en la base de datos.");
            }

            var activities = await _activityHistoryServices.GetActivityByUserEmailAsync(request.Email);

            if (usersRegistered == Guid.Empty)
            {
                throw new ApplicationException($"No existe historia para el usuario {request.Email} en la base de datos.");
            }
            return ActivityHistoryMappers.ToResponseGetList(activities);
        }
    }
}