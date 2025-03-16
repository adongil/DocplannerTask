using Docplanner.Domain.AvailavilityService;

namespace Docplanner.Infrastructure.Client
{
    public interface IAvailabilityServiceClient
    {
        Task<AvailavilityServiceResponse> GetWeeklyAvailabilityAsync(DateOnly date, string authHeader);
    }
}