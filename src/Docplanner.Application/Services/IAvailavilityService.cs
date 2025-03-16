using Docplanner.Domain.DTO;

namespace Docplanner.Application.Services
{
    public interface IAvailavilityService
    {
        Task<AvailableSlotsDTO> GetAvailableWeekSlotsAsync(DateOnly date);
    }
}