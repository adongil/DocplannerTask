using Flurl.Http.Testing;
using Docplanner.Infrastructure.Client;
using Flurl.Http;
using System.Net;
using Docplanner.Domain.AvailavilityService;

namespace Docplanner.Infrastructure.Tests.Client
{
    public class AvailabilityServiceClientTests
    {
        private const string BaseUrl = "https://draliatest.azurewebsites.net/api/availability";
        private const string AuthHeader = "Basic VGVjaHVzZXI6c2VjcmV0cGFzc1dvcmQ=";
        private readonly AvailabilityServiceClient _client;

        public AvailabilityServiceClientTests()
        {
            _client = new AvailabilityServiceClient(AuthHeader);
        }

        [Fact]
        public async Task GivenValidDate_WhenGetWeeklyAvailability_ThenReturnsSuccessWithinAServiceResponse()
        {
            using var httpTest = new HttpTest();
            httpTest
                .ForCallsTo($"https://draliatest.azurewebsites.net/api/availability/GetWeeklyAvailability/20240311")
                .WithVerb(HttpMethod.Get)
                .WithHeader("Authorization", AuthHeader)
                .RespondWith(GetAvailavilityServiceMockedJsonResponse());

            var client = new AvailabilityServiceClient(AuthHeader);
            var result = await client.GetWeeklyAvailabilityAsync(new DateOnly(2024, 3, 11));

            Assert.NotNull(result);
            AssertFacility(result.Facility);
            AssertSlotDurationMinutes(result.SlotDurationMinutes);
            AssertWorkDays(result.WorkDays);
            AssertBusySlots(result.BusySlots);
        }

        public static IEnumerable<object[]> GetErrorResponses()
        {
            yield return new object[] { HttpStatusCode.NotFound, "Not Found: The requested resource could not be found." };
            yield return new object[] { HttpStatusCode.BadRequest, "Bad Request: The request was invalid." };
            yield return new object[] { HttpStatusCode.Unauthorized, "Unauthorized: Authentication failed." };
            yield return new object[] { HttpStatusCode.InternalServerError, "Internal Server Error: There was a problem with the server." };
            yield return new object[] { HttpStatusCode.Ambiguous, "Undefined Error" };
        }

        [Theory]
        [MemberData(nameof(GetErrorResponses))]
        public async Task GivenErrorResponse_WhenGetWeeklyAvailability_ThenThrowsException(HttpStatusCode statusCode, string expectedMessage)
        {
            using var httpTest = new HttpTest();

            httpTest
                .ForCallsTo($"{BaseUrl}/GetWeeklyAvailability/20240311")
                .WithVerb(HttpMethod.Get)
                .WithHeader("Authorization", AuthHeader)
                .RespondWith(string.Empty, (int)statusCode);

            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _client.GetWeeklyAvailabilityAsync(new DateOnly(2024, 3, 11))
            );

            Assert.Equal(expectedMessage, exception.Message);
        }


        private string GetAvailavilityServiceMockedJsonResponse()
        {
            return @"
            {
                ""Facility"": {
                    ""Name"": ""Facility Example"",
                    ""Address"": ""Josep Pla 2, Edifici B2 08019 Barcelona""
                },
                ""SlotDurationMinutes"": 60,
                ""WorkDays"": {
                    ""Tuesday"": {
                        ""WorkPeriod"": {
                            ""StartHour"": 10,
                            ""EndHour"": 13,
                            ""LunchStartHour"": 17,
                            ""LunchEndHour"": 19
                        }
                    },
                    ""Thursday"": {
                        ""WorkPeriod"": {
                            ""StartHour"": 10,
                            ""EndHour"": 13,
                            ""LunchStartHour"": 17,
                            ""LunchEndHour"": 19
                        }
                    }
                },
                ""BusySlots"": [
                    { ""Start"": ""2017-06-15T10:00:00"", ""End"": ""2017-06-15T11:00:00"" },
                    { ""Start"": ""2017-06-15T11:00:00"", ""End"": ""2017-06-15T12:00:00"" },
                    { ""Start"": ""2017-06-15T17:00:00"", ""End"": ""2017-06-15T18:00:00"" }
                ]
            }";
        }


        private void AssertFacility(Facility facility)
        {
            Assert.NotNull(facility);
            Assert.Equal("Facility Example", facility.Name);
            Assert.Equal("Josep Pla 2, Edifici B2 08019 Barcelona", facility.Address);
        }

        private void AssertSlotDurationMinutes(int slotDurationMinutes)
        {
            Assert.Equal(60, slotDurationMinutes);
        }

        private void AssertWorkDays(Dictionary<string, WorkDay> workDays)
        {
            Assert.NotNull(workDays);
            Assert.Contains("Tuesday", workDays);
            Assert.Contains("Thursday", workDays);

            var tuesdayWorkPeriod = workDays["Tuesday"].WorkPeriod;
            ValidateWorkPeriod(tuesdayWorkPeriod, 10, 13, 17, 19);

            var thursdayWorkPeriod = workDays["Thursday"].WorkPeriod;
            ValidateWorkPeriod(thursdayWorkPeriod, 10, 13, 17, 19);
        }

        private void ValidateWorkPeriod(WorkPeriod workPeriod, int expectedStartHour, int expectedEndHour, int expectedLunchStart, int expectedLunchEnd)
        {
            Assert.Equal(expectedStartHour, workPeriod.StartHour);
            Assert.Equal(expectedEndHour, workPeriod.EndHour);
            Assert.Equal(expectedLunchStart, workPeriod.LunchStartHour);
            Assert.Equal(expectedLunchEnd, workPeriod.LunchEndHour);
        }

        private void AssertBusySlots(List<BusySlot> busySlots)
        {
            Assert.NotNull(busySlots);
            Assert.Equal(3, busySlots.Count);

            ValidateBusySlot(busySlots[0], new DateTime(2017, 6, 15, 10, 0, 0), new DateTime(2017, 6, 15, 11, 0, 0));
            ValidateBusySlot(busySlots[1], new DateTime(2017, 6, 15, 11, 0, 0), new DateTime(2017, 6, 15, 12, 0, 0));
            ValidateBusySlot(busySlots[2], new DateTime(2017, 6, 15, 17, 0, 0), new DateTime(2017, 6, 15, 18, 0, 0));
        }

        private void ValidateBusySlot(BusySlot busySlot, DateTime expectedStart, DateTime expectedEnd)
        {
            Assert.Equal(expectedStart, busySlot.Start);
            Assert.Equal(expectedEnd, busySlot.End);
        }
    }
}
