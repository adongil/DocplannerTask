using Docplanner.Application.Commands;
using Docplanner.Application.Services;
using Docplanner.Domain.DTO.Response;
using Docplanner.Infrastructure.Exceptions;
using MediatR;
using System.Net;

namespace Docplanner.Application.Handlers
{
    public class GetWeeklyAvailableSlotsHandler : IRequestHandler<GetWeeklyAvailableSlotsCommand, AvailableSlotsDTO>
    {
        private readonly ISlotService _slotService;

        public GetWeeklyAvailableSlotsHandler(ISlotService availavilityService)
        {
            _slotService = availavilityService;
        }

        public async Task<AvailableSlotsDTO> Handle(GetWeeklyAvailableSlotsCommand request, CancellationToken cancellationToken)
        {
            var response = await _slotService.GetAvailableWeekSlotsAsync(request.Date);

            if (response == null)
            {
                throw new AppException("No available slots found for the given week.", (int)HttpStatusCode.NotFound);
            }

            return response;
        }
    }
}
