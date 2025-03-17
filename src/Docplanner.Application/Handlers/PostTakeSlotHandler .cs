using Docplanner.Application.Commands;
using Docplanner.Application.Services;
using MediatR;

public class PostTakeSlotHandler : IRequestHandler<PostTakeSlotCommand, bool>
{
    private readonly ISlotService _availavilityService;

    public PostTakeSlotHandler(ISlotService availavilityService)
    {
        _availavilityService = availavilityService;
    }

    public async Task<bool> Handle(PostTakeSlotCommand request, CancellationToken cancellationToken)
    {
        return await _availavilityService.TakeSlot(request.Slot);
    }
}
