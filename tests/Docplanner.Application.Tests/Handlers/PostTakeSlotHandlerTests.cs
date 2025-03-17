using Docplanner.Application.Commands;
using Docplanner.Application.Services;
using Docplanner.Domain.DTO.Request;
using NSubstitute;

namespace Docplanner.Application.Tests
{
    public class PostTakeSlotHandlerTests
    {
        private readonly ISlotService _slotServiceMock;
        private readonly PostTakeSlotHandler _handler;

        public PostTakeSlotHandlerTests()
        {
            _slotServiceMock = Substitute.For<ISlotService>();
            _handler = new PostTakeSlotHandler(_slotServiceMock);
        }

        [Fact]
        public async Task GivenAValidTakeSlotCommand_WhenHandlerIsExecuted_ThenServiceIsCalledOnce()
        {
            // Arrange
            var slot = new SlotDTO();
            var command = new PostTakeSlotCommand(slot);

            _slotServiceMock.TakeSlot(slot).Returns(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _slotServiceMock.Received(1).TakeSlot(slot);
            Assert.True(result);
        }
    }
}
