using System.Text.Json.Serialization;

namespace Docplanner.Domain.AvailavilityService;

public record WorkPeriod(
    [property: JsonPropertyName("StartHour")] int StartHour,
    [property: JsonPropertyName("EndHour")] int EndHour,
    [property: JsonPropertyName("LunchStartHour")] int LunchStartHour,
    [property: JsonPropertyName("LunchEndHour")] int LunchEndHour
);