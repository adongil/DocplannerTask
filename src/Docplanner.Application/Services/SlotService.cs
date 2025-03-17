using Docplanner.Domain.AvailavilityService;
using Docplanner.Domain.DTO.Request;
using Docplanner.Domain.DTO.Response;
using Docplanner.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Docplanner.Application.Services
{
    public class SlotService : ISlotService
    {
        private readonly IAvailabilityServiceClient _availabilityServiceClient;
        private readonly ILogger<SlotService> _logger;


        public SlotService(IAvailabilityServiceClient availabilityServiceClient, ILogger<SlotService> logger)
        {
            _availabilityServiceClient = availabilityServiceClient;
            _logger = logger;

        }

        public async Task<bool> TakeSlot(SlotDTO slot)
        {
            _logger.LogInformation("Attempting to take slot: {@Slot}", slot);

            return await _availabilityServiceClient.TakeSlotAsync(slot);
        }


        public async Task<AvailableSlotsDTO?> GetAvailableWeekSlotsAsync(DateOnly date)
        {
            _logger.LogInformation("Fetching weekly available slots for date: {Date}", date);

            var availabilityResponse = await _availabilityServiceClient.GetWeeklyAvailableSlots(date);

            if (availabilityResponse == null)
            {
                _logger.LogWarning("No availability response received for date: {Date}", date);

                return null;
            }

            var availableWeekSlots = availabilityResponse.Days
                        .Select(day =>
                        {
                            var (dayOfWeek, dailyAvailability) = day;  
                            return CreateDaySlots(date, availabilityResponse, dayOfWeek, dailyAvailability);
                        })
                        .ToList();

            _logger.LogInformation("Successfully fetched available slots for date: {Date}, number of days fetched with available slots {NumDays}", date, availableWeekSlots.Count);

            return new AvailableSlotsDTO(date, availableWeekSlots);
        }


        private DaySlotsDTO CreateDaySlots(DateOnly date ,AvailavilityServiceResponse availabilityResponse, DayOfWeek dayOfWeek, DailyAvailability dailyAvailability)
        {
            var availableDate = date.AddDays((int)dayOfWeek - 1);

            var dailySlots = GenerateDailyTimeSlots(availableDate, dailyAvailability.WorkPeriod, availabilityResponse.SlotDurationMinutes);

            var availableDailySlots = FilterNotAvailableDailySlots(dailySlots, dailyAvailability.WorkPeriod, dailyAvailability.BusySlots);
                
            return new DaySlotsDTO(dayOfWeek.ToString(), availableDailySlots);
        }


        private List<DateTime> GenerateDailyTimeSlots(DateOnly date, WorkPeriod workPeriod, int slotDurationMinutes)
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

        private List<string> FilterNotAvailableDailySlots(List<DateTime> slots, WorkPeriod workPeriod, List<BusySlot>? busySlots)
        {
            var filteredSlots = slots
                .Where(slot =>
                    !(slot.Hour >= workPeriod.LunchStartHour && slot.Hour < workPeriod.LunchEndHour) &&
                    (busySlots == null ||
                    !busySlots.Any(busySlot => slot >= busySlot.Start && slot < busySlot.End)))
                .Select(dailySlot => dailySlot.ToString("yyyy-MM-dd HH:mm:ss"))
                .ToList();

            return filteredSlots;
        }
    }
}
