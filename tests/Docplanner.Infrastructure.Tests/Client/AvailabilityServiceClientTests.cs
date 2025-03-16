using Flurl.Http.Testing;
using Docplanner.Infrastructure.Client;
using System.Net;
using Docplanner.Domain.AvailavilityService;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Microsoft.AspNetCore.Http;

namespace Docplanner.Infrastructure.Tests.Client
{
    public class AvailabilityServiceClientTests
    {
        private const string BaseUrl = "https://mocked-url.com/api/availability";
        private readonly AvailabilityServiceClient _client;

        public AvailabilityServiceClientTests()
        {
            var configuration = Substitute.For<IConfiguration>();
            configuration["SlotService:BaseUrl"].Returns(BaseUrl);
            
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var httpContext = Substitute.For<HttpContext>();
            var headers = new HeaderDictionary
                {
                    { "Authorization", "Basic VGVjaHVzZXI6c2VjcmV0cGFzc1dvcmQ=" }  
                };
            httpContext.Request.Headers.Returns(headers);
            httpContextAccessor.HttpContext.Returns(httpContext); 
            
            _client = new AvailabilityServiceClient(configuration, httpContextAccessor);
        }

        [Fact]
        public async Task GivenNonMondayDate_WhenGetWeeklyAvailability_ThenThrowsHttpRequestException()
        {
            using var httpTest = new HttpTest();
            var invalidDateResponse = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">datetime must be a Monday</string>";

            httpTest
                .ForCallsTo($"{BaseUrl}/GetWeeklyAvailability/20240312") 
                .WithVerb(HttpMethod.Get)
                .RespondWith(invalidDateResponse, 400);

            var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetWeeklyAvailableSlots(new DateOnly(2024, 3, 12))
            );

            Assert.Contains("Datetime must be a Monday", exception.Message);
        }

        [Fact]
        public async Task GivenAMondayDate_WhenGetWeeklyAvailability_ThenReturnsSuccessWithinAServiceResponse()
        {
            using var httpTest = new HttpTest();
            httpTest
                .ForCallsTo($"{BaseUrl}/GetWeeklyAvailability/20240311")
                .WithVerb(HttpMethod.Get)
                .RespondWith(GetAvailavilityServiceMockedJsonResponse());

            var result = await _client.GetWeeklyAvailableSlots(new DateOnly(2024, 3, 11));

            Assert.NotNull(result);
            AssertFacility(result.Facility);
            Assert.Equal(60, result.SlotDurationMinutes);
            Assert.True(result.Days.ContainsKey(DayOfWeek.Tuesday));
            Assert.True(result.Days.ContainsKey(DayOfWeek.Thursday));
            Assert.True(result.Days.ContainsKey(DayOfWeek.Friday));
            Assert.False(result.Days.ContainsKey(DayOfWeek.Monday));
            Assert.False(result.Days.ContainsKey(DayOfWeek.Wednesday));
            Assert.False(result.Days.ContainsKey(DayOfWeek.Saturday));
            Assert.False(result.Days.ContainsKey(DayOfWeek.Sunday));
        }

        public static IEnumerable<object[]> GetErrorResponses()
        {
            yield return new object[] { HttpStatusCode.NotFound, "Not Found: The requested resource could not be found." };
            yield return new object[] { HttpStatusCode.BadRequest, "Bad Request: The request was invalid." };
            yield return new object[] { HttpStatusCode.Unauthorized, "Unauthorized: Authentication failed." };
            yield return new object[] { HttpStatusCode.InternalServerError, "Internal Server Error: There was a problem with the server." };
            yield return new object[] { (HttpStatusCode)418, "An unexpected error occurred." };
        }
        [Theory]
        [MemberData(nameof(GetErrorResponses))]
        public async Task GivenErrorResponse_WhenGetWeeklyAvailability_ThenThrowsHttpRequestException(HttpStatusCode statusCode, string expectedMessage)
        {
            using var httpTest = new HttpTest();

            httpTest
                .ForCallsTo($"{BaseUrl}/GetWeeklyAvailability/20240311")
                .WithVerb(HttpMethod.Get)
                .RespondWith(string.Empty, (int)statusCode);

            var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetWeeklyAvailableSlots(new DateOnly(2024, 3, 11))
            );

            Assert.Equal(expectedMessage, exception.Message);
        }

        public static IEnumerable<object[]> GetInvalidJsonResponses()
        {
            yield return new object[] { "", "The input does not contain any JSON tokens" };
            yield return new object[] { "banana", "is an invalid start of a value" };
        }
        [Theory]
        [MemberData(nameof(GetInvalidJsonResponses))]
        public async Task GivenInvalidJsonResponse_WhenGetWeeklyAvailability_ThenHandleJsonException(string responseContent, string expectedInnerMessage)
        {
            using var httpTest = new HttpTest();

            httpTest
                .ForCallsTo($"{BaseUrl}/GetWeeklyAvailability/20240311")
                .WithVerb(HttpMethod.Get)
                .RespondWith(responseContent, 200);

            var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetWeeklyAvailableSlots(new DateOnly(2024, 3, 11))
            );

            Assert.Contains(expectedInnerMessage, exception.InnerException?.Message);
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
                ""Tuesday"": {
                    ""WorkPeriod"": {
                        ""StartHour"": 9,
                        ""EndHour"": 17,
                        ""LunchStartHour"": 13,
                        ""LunchEndHour"": 14
                    }
                },
                ""Thursday"": {
                    ""WorkPeriod"": {
                        ""StartHour"": 9,
                        ""EndHour"": 17,
                        ""LunchStartHour"": 13,
                        ""LunchEndHour"": 14
                    },
                    ""BusySlots"": [
                        { ""Start"": ""2017-06-15T10:00:00"", ""End"": ""2017-06-15T11:00:00"" },
                        { ""Start"": ""2017-06-15T11:00:00"", ""End"": ""2017-06-15T12:00:00"" }
                    ]
                },
                ""Friday"": {
                    ""WorkPeriod"": {
                        ""StartHour"": 8,
                        ""EndHour"": 16,
                        ""LunchStartHour"": 13,
                        ""LunchEndHour"": 14
                    }
                }
            }";
        }

        private void AssertFacility(Facility facility)
        {
            Assert.NotNull(facility);
            Assert.Equal("Facility Example", facility.Name);
            Assert.Equal("Josep Pla 2, Edifici B2 08019 Barcelona", facility.Address);
        }
    }
}
