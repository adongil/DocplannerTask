using Docplanner.Application.Commands;
using Docplanner.Application.Handlers;
using Docplanner.Application.Services;
using NSubstitute;
using Docplanner.Infrastructure.Exceptions;
using Moq;
using Docplanner.Domain.DTO.Response;

namespace Docplanner.Application.Tests;

public class GetWeeklyAvailableSlotsHandlerTests
{
    private readonly ISlotService _slotServiceMock;
    private readonly GetWeeklyAvailableSlotsHandler _handler;

    public GetWeeklyAvailableSlotsHandlerTests()
    {
        _slotServiceMock = Substitute.For<ISlotService>();
        _handler = new GetWeeklyAvailableSlotsHandler(_slotServiceMock);
    }

    [Fact]
    public async Task GivenAValidGetWeeklyAvailableSlotsCommand_WhenHandlerIsExecuted_ThenServiceIsCalledOnce()
    {
        // Arrange
        var date = new DateOnly(2023, 11, 20);
        _slotServiceMock
            .GetAvailableWeekSlotsAsync(date)
            .Returns(new AvailableSlotsDTO("FacilityId",date, new List<DaySlotsDTO>()));
        var command = new GetWeeklyAvailableSlotsCommand(date);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _slotServiceMock
            .Received(1)
            .GetAvailableWeekSlotsAsync(Arg.Any<DateOnly>());
    }

    [Fact]
    public async Task GivenAValidGetWeeklyAvailableSlotsCommand_WhenServiceReturnsNull_ThenHandlerThrowsAppException()
    {
        // Arrange
        var date = new DateOnly(2023, 11, 20);
        _slotServiceMock
            .GetAvailableWeekSlotsAsync(date)
            .Returns((AvailableSlotsDTO?)null);

        var command = new GetWeeklyAvailableSlotsCommand(date);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("No available slots found for the given week.", exception.Message);
        Assert.Equal(404, exception.StatusCode);  
    }

    [Fact]
    public async Task GivenAValidGetWeeklyAvailableSlotsCommand_WhenServiceReturnsSlots_ThenHandlerReturnsAvailableSlotsDTO()
    {
        // Arrange
        var date = new DateOnly(2023, 11, 20);
        var expectedAvailableSlotsDTO = new AvailableSlotsDTO(
            "FacilityId",
            date,
            new List<DaySlotsDTO>
            {
                new DaySlotsDTO("Monday", new List<string> { "2023-11-20 09:00:00", "2023-11-20 09:30:00" }),
                new DaySlotsDTO("Tuesday", new List<string> { "2023-11-21 09:00:00", "2023-11-21 09:30:00" })
            }
        );

        _slotServiceMock
             .GetAvailableWeekSlotsAsync(date)
             .Returns(expectedAvailableSlotsDTO);

        var command = new GetWeeklyAvailableSlotsCommand(date);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAvailableSlotsDTO.Date, result.Date);
        Assert.Equal(expectedAvailableSlotsDTO.Days.Count, result.Days.Count);
        Assert.Collection(result.Days,
            day1 =>
            {
                Assert.Equal("Monday", day1.Day);
                Assert.Equal(expectedAvailableSlotsDTO.Days[0].AvailableTimeSlots, day1.AvailableTimeSlots);
            },
            day2 =>
            {
                Assert.Equal("Tuesday", day2.Day);
                Assert.Equal(expectedAvailableSlotsDTO.Days[1].AvailableTimeSlots, day2.AvailableTimeSlots);
            });
    }
}
