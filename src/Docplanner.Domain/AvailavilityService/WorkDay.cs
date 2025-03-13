using System.Text.Json.Serialization;

namespace Docplanner.Domain.AvailavilityService
{
    public record WorkDay(
        [property: JsonPropertyName("WorkPeriod")] WorkPeriod WorkPeriod
    );
}