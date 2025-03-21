﻿using Docplanner.Domain.DTO.Response;
using MediatR;

namespace Docplanner.Application.Commands;

public class GetWeeklyAvailableSlotsCommand : IRequest<AvailableSlotsDTO>
{
    public DateOnly Date { get; }

    public GetWeeklyAvailableSlotsCommand(DateOnly date)
    {
        Date = date;
    }
}
