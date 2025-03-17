﻿using Docplanner.Application.Commands;
using Docplanner.Domain.DTO.Request;
using Docplanner.Infrastructure.Exceptions;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Globalization;
using System.Text.Json;

namespace Docplanner.API.Routes
{
    public static class Routing
    {
        public static void MapSlotsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/slots/{date:}", async (string date, IMediator mediator) =>
            {
                try
                {
                    if (!DateOnly.TryParseExact(date, "yyyyMMdd", null, DateTimeStyles.None, out var dateOnly))
                    {
                        return Results.BadRequest(new { message = "Invalid date format. Please use yyyyMMdd." });
                    }

                    var command = new GetWeeklyAvailableSlotsCommand(dateOnly);
                    var result = await mediator.Send(command);

                    return Results.Ok(result);
                }
                catch (AppException ex)
                {
                    return Results.Json(new { message = ex.Message }, new JsonSerializerOptions(), null, ex.StatusCode);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { message = ex.Message }, new JsonSerializerOptions(), null, 500);
                }
            })
            .RequireAuthorization()
            .WithMetadata(new SwaggerParameterAttribute
            {
                 Description = "Date in yyyyMMdd format",
            });


            app.MapPost("/api/slots/take", async (SlotDTO slotRequest, IMediator mediator) =>
            {
                try
                {
                    var command = new PostTakeSlotCommand(slotRequest);
                    var result = await mediator.Send(command);
                    return result ? Results.Ok(new { message = "Slot taken successfully." })
                                  : Results.BadRequest(new { message = "Failed to take the slot." });
                }
                catch (AppException ex)
                {
                    return Results.Json(new { message = ex.Message }, new JsonSerializerOptions(), null, ex.StatusCode);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { message = ex.Message }, new JsonSerializerOptions(), null, 500);
                }
            })
            .RequireAuthorization();
        }
    }
}
