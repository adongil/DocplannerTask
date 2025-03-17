using Docplanner.Application.Commands;
using Docplanner.Application.Services;
using MediatR;

public class PostTakeSlotHandler : IRequestHandler<PostTakeSlotCommand, bool>
{
    private readonly IAvailavilityService _availavilityService;

    public PostTakeSlotHandler(IAvailavilityService availavilityService)
    {
        _availavilityService = availavilityService;
    }

    public async Task<bool> Handle(PostTakeSlotCommand request, CancellationToken cancellationToken)
    {
        return await _availavilityService.TakeSlot(request.Slot);
    }
}
