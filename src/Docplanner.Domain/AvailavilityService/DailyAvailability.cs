using System.Text.Json.Serialization;

namespace Docplanner.Domain.AvailavilityService
{
    public record DailyAvailability(
            [property: JsonPropertyName("WorkPeriod")] WorkPeriod WorkPeriod,
            [property: JsonPropertyName("BusySlots")] List<BusySlot>? BusySlots
        );
}
