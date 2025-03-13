using System.Text.Json.Serialization;

namespace Docplanner.Domain.AvailavilityService
{
    public record BusySlot(
        [property: JsonPropertyName("Start")] DateTime? Start,
        [property: JsonPropertyName("End")] DateTime? End
    );
}