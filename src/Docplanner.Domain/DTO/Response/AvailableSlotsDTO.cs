namespace Docplanner.Domain.DTO.Response
{
    public record AvailableSlotsDTO(
        DateOnly Date,
        List<DaySlotsDTO> Days
    );
}
