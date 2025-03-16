using Docplanner.Application.Commands;
using Docplanner.Application.Handlers;
using Docplanner.Application.Services;
using Docplanner.Domain.DTO;
using Moq;

namespace Docplanner.Application.Tests.Handlers
{
    public class GetWeeklyAvailableSlotsHandlerTests
    {
        private readonly Mock<IAvailavilityService> _availavilityServiceMock;
        private readonly GetWeeklyAvailableSlotsHandler _handler;

        public GetWeeklyAvailableSlotsHandlerTests()
        {
            _availavilityServiceMock = new Mock<IAvailavilityService>();
            _handler = new GetWeeklyAvailableSlotsHandler(_availavilityServiceMock.Object);
        }

        [Fact]
        public async Task GivenAValidGetWeeklyAvailableSlotsCommand_WhenHandlerIsExecuted_ThenServiceIsCalledOnce()
        {
            // Arrange
            var date = new DateOnly(2023, 11, 20);
            var command = new GetWeeklyAvailableSlotsCommand(date);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _availavilityServiceMock.Verify(service => service.GetAvailableWeekSlotsAsync(It.IsAny<DateOnly>()), Times.Once);
        }

        [Fact]
        public async Task GivenAValidGetWeeklyAvailableSlotsCommand_WhenServiceReturnsNull_ThenHandlerReturnsNull()
        {
            // Arrange
            var date = new DateOnly(2023, 11, 20);
            _availavilityServiceMock
                .Setup(service => service.GetAvailableWeekSlotsAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync((AvailableSlotsDTO)null);

            var command = new GetWeeklyAvailableSlotsCommand(date);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GivenAValidGetWeeklyAvailableSlotsCommand_WhenHandlerIsExecuted_ThenItReturnsAvailableSlotsDTO()
        {
            // Arrange
            var date = new DateOnly(2023, 11, 20);
            var expectedAvailableSlotsDTO = new AvailableSlotsDTO(
                date,
                new List<DaySlotsDTO>
                {
                    new DaySlotsDTO("Monday", new List<string> { "2023-11-20 09:00:00", "2023-11-20 09:30:00" }),
                    new DaySlotsDTO("Tuesday", new List<string> { "2023-11-21 09:00:00", "2023-11-21 09:30:00" })
                }
            );

            _availavilityServiceMock
                .Setup(service => service.GetAvailableWeekSlotsAsync(It.IsAny<DateOnly>()))
                .ReturnsAsync(expectedAvailableSlotsDTO);

            var command = new GetWeeklyAvailableSlotsCommand(date);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAvailableSlotsDTO.Date, result.Date);
            Assert.Equal(expectedAvailableSlotsDTO.Days.Count, result.Days.Count);
            Assert.Equal("Monday", result.Days[0].Day);
            Assert.Equal("Tuesday", result.Days[1].Day);
        }
    }
}
