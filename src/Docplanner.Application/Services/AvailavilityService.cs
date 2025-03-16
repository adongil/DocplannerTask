using Docplanner.Domain.AvailavilityService;
using Docplanner.Domain.DTO;
using Docplanner.Infrastructure.Client;

namespace Docplanner.Application.Services
{
    public class AvailavilityService : IAvailavilityService
    {
        private readonly IAvailabilityServiceClient _availabilityServiceClient;

        public AvailavilityService(IAvailabilityServiceClient availabilityServiceClient)
        {
            _availabilityServiceClient = availabilityServiceClient;
        }


        public async Task<AvailableSlotsDTO> GetAvailableWeekSlotsAsync(DateOnly date)
        {
            var availabilityResponse = await _availabilityServiceClient.GetWeeklyAvailableSlots(date);

            if (availabilityResponse == null)
            {
                return new AvailableSlotsDTO(date, new List<DaySlotsDTO>());
            }

            var availableWeekSlots = new List<DaySlotsDTO>();

            foreach (var (dayOfWeek, dailyAvailability) in availabilityResponse.Days)
            {
                var dailySlots = new List<DateTime>();

                var startHour = new DateTime(date.Year, date.Month, date.Day, dailyAvailability.WorkPeriod.StartHour, 0, 0);
                var endHour = new DateTime(date.Year, date.Month, date.Day, dailyAvailability.WorkPeriod.EndHour, 0, 0);

                for (var time = startHour; time < endHour; time = time.AddMinutes(availabilityResponse.SlotDurationMinutes))
                {
                    dailySlots.Add(time);
                }

                dailySlots = FilterNotAvailableSlots(dailySlots, dailyAvailability);

                var availableSlots = dailySlots.Select(dailySlot => dailySlot.ToString("yyyy-MM-dd HH:mm:ss")).ToList();

                availableWeekSlots.Add(new DaySlotsDTO(dayOfWeek.ToString(), availableSlots));
            }

            return new AvailableSlotsDTO(date, availableWeekSlots);
        }

        private List<DateTime> FilterNotAvailableSlots(List<DateTime> slots, DailyAvailability dailyAvailability)
        {
            var filteredSlots = slots
                .Where(slot =>
                    !(slot.Hour >= dailyAvailability.WorkPeriod.LunchStartHour && slot.Hour < dailyAvailability.WorkPeriod.LunchEndHour) &&
                    (dailyAvailability.BusySlots == null ||
                    !dailyAvailability.BusySlots.Any(busySlot => slot >= busySlot.Start && slot < busySlot.End)))
                .ToList();

            return filteredSlots;
        }

    }
}
