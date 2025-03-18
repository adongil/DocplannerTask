namespace Docplanner.Domain.DTO.Response;

public record DaySlotsDTO(
    string Day,
    List<string> AvailableTimeSlots
);
