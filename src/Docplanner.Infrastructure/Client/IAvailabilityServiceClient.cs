using Docplanner.Domain.AvailavilityService;

namespace Docplanner.Infrastructure.Client
{
    public interface IAvailabilityServiceClient
    {
        Task<AvailavilityServiceResponse> GetWeeklyAvailableSlots(DateOnly date);
    }
}