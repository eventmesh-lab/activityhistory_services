using activityhistory_application.Commands.Commands;
using activityhistory_application.DTOs;
using activityhistory_application.Interfaces;
using activityhistory_application.Queries.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace activityhistory_api.Controllers
{
    [ApiController]
    [Route("api/activityhistory")]
    public class ActivityHistoryControllers : ControllerBase

    {
        private readonly IMediator _mediator;
        public ActivityHistoryControllers( IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("registerActivity/{email}")]
        public async Task<IActionResult> CreateActivity(string email, [FromBody] CreateActivityDTO request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Llegó al controlador");
            Console.WriteLine($"Request Data: {request.Action}");

            var command = new CreateActivityCommand(request, email);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new { Usuario = response, Mensaje = "Actividad registrada exitosamente." });
        }

        [HttpGet("getHistory/{email}")]
        public async Task<IActionResult> GetHistoryByUserEmail(string email, CancellationToken cancellationToken)
        {
            Console.WriteLine("Llegó al controlador");

            var command = new GetActivitiesByUserEmailQuery(email);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }


    }
}
