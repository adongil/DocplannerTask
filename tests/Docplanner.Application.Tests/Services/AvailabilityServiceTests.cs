using Docplanner.Application.Services;
using Docplanner.Domain.AvailavilityService;
using Docplanner.Domain.DTO.Request;
using Docplanner.Infrastructure.Client;
using Docplanner.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Docplanner.Application.Tests.Services;

public class AvailabilityServiceTests
{
    private readonly IAvailabilityServiceClient _availabilityServiceClient;
    private readonly SlotService _slotService;
    private const string AuthHeader = "Basic VGVjaHVzZXI6c2VjcmV0cGFzc1dvcmQ=";
    private readonly ILogger<SlotService> _logger;

    public AvailabilityServiceTests()
    {
        _logger = Substitute.For<ILogger<SlotService>>();
        _availabilityServiceClient = Substitute.For<IAvailabilityServiceClient>();
        _slotService = new SlotService(_availabilityServiceClient, _logger);
    }


    [Fact]
    public async Task GivenAValidDate_WhenGettingWeeklyAvailability_ThenClientIsCalledOnce()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 14);
        var mockResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Test Facility", "Test Address"), 60
        );
        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate)
            .Returns(mockResponse);

        // Act
        await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        await _availabilityServiceClient.Received(1).GetWeeklyAvailableSlots(requestedDate);
    }

    [Fact]
    public async Task GivenNullAvailabilityResponse_WhenGettingWeeklyAvailability_ThenReturnsNull()
    {
        var requestedDate = new DateOnly(2024, 3, 14);
        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate)
            .Returns((AvailavilityServiceResponse?)null);

        var result = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        Assert.Null(result);
    }


    [Fact]
    public async Task GivenAValidDateAndADefinedRangeOfWorkingHours_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsInDefinedRange()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            60)
            {
                RawDays = new Dictionary<string, JsonElement>
                {
                    { "Wednesday", JsonSerializer.SerializeToElement(new DailyAvailability(new WorkPeriod(9, 17, 0, 0), null)) }
                }
            };
        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        var expectedTimeSlots = new List<string>
            {
                "2024-03-12 09:00:00","2024-03-12 10:00:00","2024-03-12 11:00:00","2024-03-12 12:00:00",
                "2024-03-12 13:00:00","2024-03-12 14:00:00","2024-03-12 15:00:00","2024-03-12 16:00:00"
            };

        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Wednesday", availableWeekSlots.Days.First().Day);
        Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
    }


    [Fact]
    public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithLuchBreak_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsInDefinedRange()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            60)
        {
            RawDays = new Dictionary<string, JsonElement>
                {
                    { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(new WorkPeriod(9, 17, 13, 14), null)) }
                }
        };
        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        var expectedTimeSlots = new List<string>
            {
                "2024-03-11 09:00:00","2024-03-11 10:00:00","2024-03-11 11:00:00","2024-03-11 12:00:00",
                "2024-03-11 14:00:00","2024-03-11 15:00:00","2024-03-11 16:00:00"
            };

        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
        Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
    }


    [Fact]
    public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithLuchBreakAndBusySlots_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsInDefinedRange()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            60)
        {
            RawDays = new Dictionary<string, JsonElement>
                    {
                        { "Friday", JsonSerializer.SerializeToElement(new DailyAvailability(
                            new WorkPeriod(9, 17, 13, 14),
                            new List<BusySlot>
                            {
                                new BusySlot(new DateTime(2024, 3, 14, 10, 0, 0), new DateTime(2024, 3, 14, 11, 0, 0))
                            }))
                        }
                    }
        };
        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        var expectedTimeSlots = new List<string>
            {
                "2024-03-14 09:00:00","2024-03-14 11:00:00","2024-03-14 12:00:00",
                "2024-03-14 14:00:00","2024-03-14 15:00:00","2024-03-14 16:00:00"
            };

        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Friday", availableWeekSlots.Days.First().Day);
        Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
    }


    [Fact]
    public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithZeroDuration_WhenGettingWeeklyAvailability_ThenReturnNoAvailableSlots()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            60)
        {
            RawDays = new Dictionary<string, JsonElement>
            {
                { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(
                    new WorkPeriod(0, 0, 0, 0), 
                    null)) }
            }
        };
        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
        Assert.Empty(availableWeekSlots.Days.First().AvailableTimeSlots); 
    }

    [Fact]
    public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithOverlappingBusySlots_WhenGettingWeeklyAvailability_ThenReturnValidSlotsExcludingOverlappingSlots()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            60)
        {
            RawDays = new Dictionary<string, JsonElement>
            {
                { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(
                    new WorkPeriod(9, 17, 13, 14),
                    new List<BusySlot>
                    {
                        new BusySlot(new DateTime(2024, 3, 11, 10, 0, 0), new DateTime(2024, 3, 11, 12, 0, 0)),
                        new BusySlot(new DateTime(2024, 3, 11, 11, 0, 0), new DateTime(2024, 3, 11, 13, 0, 0))
                    }))
                }
            }
        };
        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        var expectedTimeSlots = new List<string>
        {
            "2024-03-11 09:00:00", "2024-03-11 14:00:00", "2024-03-11 15:00:00", "2024-03-11 16:00:00"
        };

        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
        Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
    }

    [Fact]
    public async Task GivenAValidDateAndANegativeWorkPeriod_WhenGettingWeeklyAvailability_ThenReturnNoAvailableSlots()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            60) 
        {
            RawDays = new Dictionary<string, JsonElement>
            {
                { "Monday", JsonSerializer.SerializeToElement(new DailyAvailability(
                    new WorkPeriod(-1, -5, 13, 14), null)) }  
            }
        };

        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Monday", availableWeekSlots.Days.First().Day);
        Assert.Empty(availableWeekSlots.Days.First().AvailableTimeSlots);  
    }

    [Fact]
    public async Task GivenNegativeSlotDuration_WhenGettingWeeklyAvailability_ThenReturnsNoAvailableSlots()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            -30) 
        {
            RawDays = new Dictionary<string, JsonElement>
            {
                { "Wednesday", JsonSerializer.SerializeToElement(new DailyAvailability(
                    new WorkPeriod(9, 17, 13, 14), null)) }
            }
        };

        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Wednesday", availableWeekSlots.Days.First().Day);
        Assert.Empty(availableWeekSlots.Days.First().AvailableTimeSlots);  
    }

    [Fact]
    public async Task GivenBusySlotsOutsideWorkPeriod_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsIgnoringBusySlots()
    {
        // Arrange
        var requestedDate = new DateOnly(2024, 3, 10);
        var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
            new Facility("FacilityId", "Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
            60)
        {
            RawDays = new Dictionary<string, JsonElement>
            {
                { "Thursday", JsonSerializer.SerializeToElement(new DailyAvailability(
                    new WorkPeriod(9, 17, 13, 14), 
                    new List<BusySlot>
                    {
                        new BusySlot(new DateTime(2024, 3, 14, 7, 0, 0), new DateTime(2024, 3, 14, 8, 0, 0)), 
                        new BusySlot(new DateTime(2024, 3, 14, 18, 0, 0), new DateTime(2024, 3, 14, 19, 0, 0)) 
                    }))
                }
            }
        };

        _availabilityServiceClient.GetWeeklyAvailableSlots(requestedDate).Returns(weeklyAvailabilityResponse);

        // Act
        var availableWeekSlots = await _slotService.GetAvailableWeekSlotsAsync(requestedDate);

        // Assert
        var expectedTimeSlots = new List<string>
        {
            "2024-03-13 09:00:00", "2024-03-13 10:00:00", "2024-03-13 11:00:00", "2024-03-13 12:00:00",
            "2024-03-13 14:00:00", "2024-03-13 15:00:00", "2024-03-13 16:00:00"
        };

        Assert.Single(availableWeekSlots.Days);
        Assert.Equal("Thursday", availableWeekSlots.Days.First().Day);
        Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
    }

    [Fact]
    public async Task GivenValidSlotDetails_WhenTakeSlotIsCalled_ThenTakeSlotAsyncIsCalledOnce()
    {
        // Arrange
        _availabilityServiceClient.TakeSlotAsync(It.IsAny<SlotDTO>()).Returns(It.IsAny<bool>());

        // Act
        await _slotService.TakeSlot(It.IsAny<SlotDTO>());

        // Assert
        await _availabilityServiceClient.Received(1).TakeSlotAsync(It.IsAny<SlotDTO>());
    }

    [Fact]
    public async Task GivenValidSlotDetails_WhenTakeSlotIsCalled_ThenReturnsTrue()
    {
        // Arrange
        var slot = new SlotDTO
        {
            Start = "2024-03-11 09:00:00",
            End = "2024-03-11 10:00:00",
            Comments = "arm pain",
            Patient = new PatientDTO
            {
                Name = "Mario",
                SecondName = "Neta",
                Email = "mario.neta@example.com",
                Phone = "555 44 33 22"
            }
        };

        _availabilityServiceClient.TakeSlotAsync(slot).Returns(true);

        // Act
        var result = await _slotService.TakeSlot(slot);

        // Assert
        Assert.True(result);
    }


    [Fact]
    public async Task GivenErrorWhenCallingTakeSlot_WhenTakeSlotFails_ThenExceptionIsForwarded()
    {
        // Arrange
        _availabilityServiceClient.TakeSlotAsync(It.IsAny<SlotDTO>())
            .Throws(new AppException("Error message",500));

        // Act
        var exception = await Assert.ThrowsAsync<AppException>(async () =>
            await _slotService.TakeSlot(It.IsAny<SlotDTO>()));

        // Assert
        Assert.Equal("Error message",exception.Message);
        Assert.Equal(500, exception.StatusCode);
    }
}

