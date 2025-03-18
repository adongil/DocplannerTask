namespace Docplanner.Domain.DTO.Request;

public class SlotDTO
{
    public string FacilityId { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public string Comments { get; set; }
    public PatientDTO Patient { get; set; }
}
