using activityhistory_application.Commands.Commands;
using activityhistory_application.DTOs.DTOResponse;
using activityhistory_application.Interfaces;
using activityhistory_application.Mappers;
using activityhistory_domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_application.Commands.Handlers
{
    public class CreateActivityHandler: IRequestHandler<CreateActivityCommand, CreateActivityResponseDto>
    {
        public readonly IMediator _mediator;
        public readonly IActivityHistoryRepositoryPostgres _activityHistoryServices;
        public readonly IUserServices _userServices;
        public CreateActivityHandler(IActivityHistoryRepositoryPostgres activityHistoryServices, IUserServices userServices)
        {
            _activityHistoryServices= activityHistoryServices;
            _userServices= userServices;
        }
        public async Task<CreateActivityResponseDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
        {

            var activity = ActivityHistoryMappers.ToDomain(request);

           
            try
            {
                var usersRegistered = await _userServices.GetIdUserByEmailServices(request.UserEmail);
                Console.WriteLine($"Usuario obtenido: {usersRegistered}");
                if (usersRegistered == Guid.Empty) 
                { 
                    throw new ApplicationException($"El usuario con email {activity.UserEmail} ya existe en la base de datos.");
                }
                await _activityHistoryServices.AddActivity(activity, cancellationToken);
            }
            catch (Exception ex)
            {

                throw new ApplicationException($"No se pudo registar la actividad en la base de datos", ex);
            }
            
            Console.WriteLine("Actividad registrada.");
            return ActivityHistoryMappers.ToDto(activity);
        }
    }
}
