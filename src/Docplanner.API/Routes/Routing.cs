using Docplanner.Application.Commands;
using MediatR;

namespace Docplanner.API.Routes
{
    public static class Routing
    {
        public static void MapSlotsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/slots", async (DateOnly date, IMediator mediator) =>
            {
                var command = new GetWeeklyAvailableSlotsCommand(date);
                var result = await mediator.Send(command);
                return Results.Ok(result);  
            })
            .RequireAuthorization();  
        }
    }
}
