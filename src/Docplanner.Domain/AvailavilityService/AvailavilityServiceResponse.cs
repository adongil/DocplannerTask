using System.Text.Json.Serialization;


namespace Docplanner.Domain.AvailavilityService
{
    public record AvailavilityServiceResponse
    (
        [property: JsonPropertyName("Facility")] Facility Facility,
        [property: JsonPropertyName("SlotDurationMinutes")] int SlotDurationMinutes,
        [property: JsonPropertyName("WorkDays")] Dictionary<string, WorkDay> WorkDays,
        [property: JsonPropertyName("BusySlots")] List<BusySlot> BusySlots
    );
}


