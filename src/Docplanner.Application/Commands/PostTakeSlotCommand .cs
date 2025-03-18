using Docplanner.Domain.DTO.Request;
using MediatR;

namespace Docplanner.Application.Commands;

public class PostTakeSlotCommand : IRequest<bool>
{
    public SlotDTO Slot { get; }

    public PostTakeSlotCommand(SlotDTO slot)
    {
        Slot = slot;
    }
}
