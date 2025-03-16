using Docplanner.Application.Services;
using Docplanner.Domain.AvailavilityService;
using Docplanner.Infrastructure.Client;
using Moq;
using NSubstitute;
using System.Text.Json;

namespace Docplanner.Application.Tests.Services
{
    public class AvailabilityServiceTests
    {
        private readonly IAvailabilityServiceClient _availabilityServiceClient;
        private readonly AvailavilityService _availabilityService;
        private const string AuthHeader = "Basic VGVjaHVzZXI6c2VjcmV0cGFzc1dvcmQ=";

        public AvailabilityServiceTests()
        {
            _availabilityServiceClient = Substitute.For<IAvailabilityServiceClient>();
            _availabilityService = new AvailavilityService(_availabilityServiceClient, AuthHeader);
        }


        [Fact]
        public async Task GivenAValidDate_WhenGettingWeeklyAvailability_ThenClientIsCalledOnce()
        {
            // Arrange
            var requestedDate = new DateOnly(2024, 3, 14);

            _availabilityServiceClient.GetWeeklyAvailabilityAsync(requestedDate, AuthHeader)
                .Returns(It.IsAny<AvailavilityServiceResponse>());


            // Act
            await _availabilityService.GetAvailableWeekSlotsAsync(requestedDate);

            // Assert
            await _availabilityServiceClient.Received(1).GetWeeklyAvailabilityAsync(requestedDate, AuthHeader);
        }


        [Fact]
        public async Task GivenAValidDateAndADefinedRangeOfWorkingHours_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsInDefinedRange()
        {
            // Arrange
            var requestedDate = new DateOnly(2024, 3, 14);
            var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
                new Facility("Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
                60)
                {
                    DayCandidate = new Dictionary<string, JsonElement>
                    {
                        { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(new WorkPeriod(9, 17, 0, 0), null)) }
                    }
                };
            _availabilityServiceClient.GetWeeklyAvailabilityAsync(requestedDate, AuthHeader).Returns(weeklyAvailabilityResponse);

            // Act
            var availableWeekSlots = await _availabilityService.GetAvailableWeekSlotsAsync(requestedDate);

            // Assert
            var expectedTimeSlots = new List<string>
                {
                    "2024-03-14 09:00:00","2024-03-14 10:00:00","2024-03-14 11:00:00","2024-03-14 12:00:00",
                    "2024-03-14 13:00:00","2024-03-14 14:00:00","2024-03-14 15:00:00","2024-03-14 16:00:00"
                };

            Assert.Single(availableWeekSlots.Days);
            Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
            Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
        }


        [Fact]
        public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithLuchBreak_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsInDefinedRange()
        {
            // Arrange
            var requestedDate = new DateOnly(2024, 3, 14);
            var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
                new Facility("Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
                60)
            {
                DayCandidate = new Dictionary<string, JsonElement>
                    {
                        { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(new WorkPeriod(9, 17, 13, 14), null)) }
                    }
            };
            _availabilityServiceClient.GetWeeklyAvailabilityAsync(requestedDate, AuthHeader).Returns(weeklyAvailabilityResponse);

            // Act
            var availableWeekSlots = await _availabilityService.GetAvailableWeekSlotsAsync(requestedDate);

            // Assert
            var expectedTimeSlots = new List<string>
                {
                    "2024-03-14 09:00:00","2024-03-14 10:00:00","2024-03-14 11:00:00","2024-03-14 12:00:00",
                    "2024-03-14 14:00:00","2024-03-14 15:00:00","2024-03-14 16:00:00"
                };

            Assert.Single(availableWeekSlots.Days);
            Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
            Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
        }


        [Fact]
        public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithLuchBreakAndBusySlots_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsInDefinedRange()
        {
            // Arrange
            var requestedDate = new DateOnly(2024, 3, 14);
            var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
                new Facility("Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
                60)
            {
                DayCandidate = new Dictionary<string, JsonElement>
                        {
                            { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(
                                new WorkPeriod(9, 17, 13, 14),
                                new List<BusySlot>
                                {
                                    new BusySlot(new DateTime(2024, 3, 14, 10, 0, 0), new DateTime(2024, 3, 14, 11, 0, 0))
                                }))
                            }
                        }
            };
            _availabilityServiceClient.GetWeeklyAvailabilityAsync(requestedDate, AuthHeader).Returns(weeklyAvailabilityResponse);

            // Act
            var availableWeekSlots = await _availabilityService.GetAvailableWeekSlotsAsync(requestedDate);

            // Assert
            var expectedTimeSlots = new List<string>
                {
                    "2024-03-14 09:00:00","2024-03-14 11:00:00","2024-03-14 12:00:00",
                    "2024-03-14 14:00:00","2024-03-14 15:00:00","2024-03-14 16:00:00"
                };

            Assert.Single(availableWeekSlots.Days);
            Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
            Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
        }


        [Fact]
        public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithLunchBreakAndNoBookedSlots_WhenGettingWeeklyAvailability_ThenReturnAllValidSlotsInDefinedRange()
        {
            // Arrange
            var requestedDate = new DateOnly(2024, 3, 14);
            var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
                new Facility("Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
                60)
            {
                DayCandidate = new Dictionary<string, JsonElement>
                {
                    { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(
                        new WorkPeriod(9, 17, 13, 14),
                        null)) }
                }
            };
            _availabilityServiceClient.GetWeeklyAvailabilityAsync(requestedDate, AuthHeader).Returns(weeklyAvailabilityResponse);

            // Act
            var availableWeekSlots = await _availabilityService.GetAvailableWeekSlotsAsync(requestedDate);

            // Assert
            var expectedTimeSlots = new List<string>
            {
                "2024-03-14 09:00:00", "2024-03-14 10:00:00", "2024-03-14 11:00:00", "2024-03-14 12:00:00",
                "2024-03-14 14:00:00", "2024-03-14 15:00:00", "2024-03-14 16:00:00"
            };

            Assert.Single(availableWeekSlots.Days);
            Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
            Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
        }


        [Fact]
        public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithZeroDuration_WhenGettingWeeklyAvailability_ThenReturnNoAvailableSlots()
        {
            // Arrange
            var requestedDate = new DateOnly(2024, 3, 14);
            var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
                new Facility("Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
                60)
            {
                DayCandidate = new Dictionary<string, JsonElement>
                {
                    { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(
                        new WorkPeriod(0, 0, 0, 0), 
                        null)) }
                }
            };
            _availabilityServiceClient.GetWeeklyAvailabilityAsync(requestedDate, AuthHeader).Returns(weeklyAvailabilityResponse);

            // Act
            var availableWeekSlots = await _availabilityService.GetAvailableWeekSlotsAsync(requestedDate);

            // Assert
            Assert.Single(availableWeekSlots.Days);
            Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
            Assert.Empty(availableWeekSlots.Days.First().AvailableTimeSlots); 
        }

        [Fact]
        public async Task GivenAValidDateAndADefinedRangeOfWorkingHoursWithOverlappingBusySlots_WhenGettingWeeklyAvailability_ThenReturnValidSlotsExcludingOverlappingSlots()
        {
            // Arrange
            var requestedDate = new DateOnly(2024, 3, 14);
            var weeklyAvailabilityResponse = new AvailavilityServiceResponse(
                new Facility("Facility Example", "Josep Pla 2, Edifici B2 08019 Barcelona"),
                60)
            {
                DayCandidate = new Dictionary<string, JsonElement>
                {
                    { "Tuesday", JsonSerializer.SerializeToElement(new DailyAvailability(
                        new WorkPeriod(9, 17, 13, 14),
                        new List<BusySlot>
                        {
                            new BusySlot(new DateTime(2024, 3, 14, 10, 0, 0), new DateTime(2024, 3, 14, 12, 0, 0)),
                            new BusySlot(new DateTime(2024, 3, 14, 11, 0, 0), new DateTime(2024, 3, 14, 13, 0, 0))
                        }))
                    }
                }
            };
            _availabilityServiceClient.GetWeeklyAvailabilityAsync(requestedDate, AuthHeader).Returns(weeklyAvailabilityResponse);

            // Act
            var availableWeekSlots = await _availabilityService.GetAvailableWeekSlotsAsync(requestedDate);

            // Assert
            var expectedTimeSlots = new List<string>
            {
                "2024-03-14 09:00:00", "2024-03-14 14:00:00", "2024-03-14 15:00:00", "2024-03-14 16:00:00"
            };

            Assert.Single(availableWeekSlots.Days);
            Assert.Equal("Tuesday", availableWeekSlots.Days.First().Day);
            Assert.Equal(expectedTimeSlots, availableWeekSlots.Days.First().AvailableTimeSlots);
        }


    }
}
