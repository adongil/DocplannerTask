using Docplanner.Application.Commands;
using Docplanner.Application.Services;
using MediatR;

public class PostTakeSlotHandler : IRequestHandler<PostTakeSlotCommand, bool>
{
    private readonly ISlotService _slotService;

    public PostTakeSlotHandler(ISlotService availavilityService)
    {
        _slotService = availavilityService;
    }

    public async Task<bool> Handle(PostTakeSlotCommand request, CancellationToken cancellationToken)
    {
        return await _slotService.TakeSlot(request.Slot);
    }
}
