namespace Docplanner.Domain.DTO.Response
{
    public record AvailableSlotsDTO(
        string FacilityId,
        DateOnly Date,
        List<DaySlotsDTO> Days
    );
}
