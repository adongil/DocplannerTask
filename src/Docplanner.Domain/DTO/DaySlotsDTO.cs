namespace Docplanner.Domain.DTO
{
    public record DaySlotsDTO(
        string Day,
        List<string> AvailableTimeSlots
    );
}
