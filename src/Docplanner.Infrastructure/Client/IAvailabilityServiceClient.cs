using Docplanner.Domain.AvailavilityService;
using Docplanner.Domain.DTO.Request;

namespace Docplanner.Infrastructure.Client
{
    public interface IAvailabilityServiceClient
    {
        Task<AvailavilityServiceResponse> GetWeeklyAvailableSlots(DateOnly date);
        Task<bool> TakeSlotAsync(SlotDTO slot);
    }
}