using System.Text.Json;
using System.Text.Json.Serialization;


namespace Docplanner.Domain.AvailavilityService
{
    public record AvailavilityServiceResponse(
        [property: JsonPropertyName("Facility")] Facility Facility,
        [property: JsonPropertyName("SlotDurationMinutes")] int SlotDurationMinutes)
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? DayCandidate { get; init; }

        public Dictionary<DayOfWeek, DailyAvailability> Days =>
            DayCandidate?
                .Where(p => Enum.TryParse<DayOfWeek>(p.Key, out _))
                .ToDictionary(
                    p => Enum.Parse<DayOfWeek>(p.Key),
                    p => JsonSerializer.Deserialize<DailyAvailability>(p.Value.GetRawText())!
                ) ?? new();
    }
}


