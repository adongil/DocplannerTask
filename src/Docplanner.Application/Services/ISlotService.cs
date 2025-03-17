using Docplanner.Domain.DTO.Request;
using Docplanner.Domain.DTO.Response;

namespace Docplanner.Application.Services
{
    public interface ISlotService
    {
        Task<AvailableSlotsDTO?> GetAvailableWeekSlotsAsync(DateOnly date);
        Task<bool> TakeSlot(SlotDTO slot);
    }
}