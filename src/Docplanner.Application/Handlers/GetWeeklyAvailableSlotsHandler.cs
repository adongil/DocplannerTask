using Docplanner.Application.Commands;
using Docplanner.Application.Services;
using Docplanner.Domain.DTO;
using MediatR;

namespace Docplanner.Application.Handlers
{
    public class GetWeeklyAvailableSlotsHandler : IRequestHandler<GetWeeklyAvailableSlotsCommand, AvailableSlotsDTO>
    {
        private readonly IAvailavilityService _availavilityService;

        public GetWeeklyAvailableSlotsHandler(IAvailavilityService availavilityService)
        {
            _availavilityService = availavilityService;
        }

        public async Task<AvailableSlotsDTO> Handle(GetWeeklyAvailableSlotsCommand request, CancellationToken cancellationToken)
        {
            return await _availavilityService.GetAvailableWeekSlotsAsync(request.Date);
        }
    }
}
