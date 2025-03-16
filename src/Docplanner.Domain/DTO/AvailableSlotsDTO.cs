using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docplanner.Domain.DTO
{
    public record AvailableSlotsDTO(
        DateOnly Date,
        List<DaySlotsDTO> Days
    );
}
