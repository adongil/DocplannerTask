using System.Text.Json.Serialization;

namespace Docplanner.Domain.AvailavilityService
{
    public record Facility(
        [property: JsonPropertyName("FacilityId")] string FacilityId,
        [property: JsonPropertyName("Name")] string Name,
        [property: JsonPropertyName("Address")] string Address
    );
}