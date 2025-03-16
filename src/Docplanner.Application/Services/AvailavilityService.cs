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
                var currentDate = date.AddDays((int)dayOfWeek - 1);

                var dailySlots = CalculateDailySlots(currentDate, dailyAvailability.WorkPeriod, availabilityResponse.SlotDurationMinutes);

                var availableDailySlots = FilterNotAvailableDailySlots(dailySlots, dailyAvailability)
                    .Select(dailySlot => dailySlot.ToString("yyyy-MM-dd HH:mm:ss")).ToList();

                availableWeekSlots.Add(new DaySlotsDTO(dayOfWeek.ToString(), availableDailySlots));
            }

            return new AvailableSlotsDTO(date, availableWeekSlots);
        }

        private List<DateTime> CalculateDailySlots(DateOnly date, WorkPeriod workPeriod, int slotDurationMinutes)
        {
            var dailySlots = new List<DateTime>();

            var startHour = new DateTime(date.Year, date.Month, date.Day, workPeriod.StartHour, 0, 0);
            var endHour = new DateTime(date.Year, date.Month, date.Day, workPeriod.EndHour, 0, 0);

            for (var time = startHour; time < endHour; time = time.AddMinutes(slotDurationMinutes))
            {
                dailySlots.Add(time);
            }

            return dailySlots;
        }

        private List<DateTime> FilterNotAvailableDailySlots(List<DateTime> slots, DailyAvailability dailyAvailability)
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
