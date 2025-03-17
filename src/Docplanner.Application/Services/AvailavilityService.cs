using Docplanner.Domain.AvailavilityService;
using Docplanner.Domain.DTO;
using Docplanner.Infrastructure.Client;
using System;

namespace Docplanner.Application.Services
{
    public class AvailavilityService : IAvailavilityService
    {
        private readonly IAvailabilityServiceClient _availabilityServiceClient;

        public AvailavilityService(IAvailabilityServiceClient availabilityServiceClient)
        {
            _availabilityServiceClient = availabilityServiceClient;
        }

        public async Task<AvailableSlotsDTO?> GetAvailableWeekSlotsAsync(DateOnly date)
        {
            var availabilityResponse = await _availabilityServiceClient.GetWeeklyAvailableSlots(date);

            if (availabilityResponse == null)
            {
                return null;
            }

            var availableWeekSlots = availabilityResponse.Days
                        .Select(day =>
                        {
                            var (dayOfWeek, dailyAvailability) = day;  
                            return CreateDaySlots(date, availabilityResponse, dayOfWeek, dailyAvailability);
                        })
                        .ToList();

            return new AvailableSlotsDTO(date, availableWeekSlots);
        }


        private DaySlotsDTO CreateDaySlots(DateOnly date ,AvailavilityServiceResponse availabilityResponse, DayOfWeek dayOfWeek, DailyAvailability dailyAvailability)
        {
            var availableDate = date.AddDays((int)dayOfWeek - 1);

            var dailySlots = CalculateDailySlots(availableDate, dailyAvailability.WorkPeriod, availabilityResponse.SlotDurationMinutes);
           
            var availableDailySlots = FilterNotAvailableDailySlots(dailySlots, dailyAvailability)
                .Select(dailySlot => dailySlot.ToString("yyyy-MM-dd HH:mm:ss"))
                .ToList();

            return new DaySlotsDTO(dayOfWeek.ToString(), availableDailySlots);
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
