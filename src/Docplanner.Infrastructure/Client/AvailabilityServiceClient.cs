using System.Net;
using System.Text.Json;
using Flurl.Http;
using Docplanner.Domain.AvailavilityService;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Docplanner.Infrastructure.Exceptions;
using Docplanner.Domain.DTO.Request;

namespace Docplanner.Infrastructure.Client
{
    public class AvailabilityServiceClient : IAvailabilityServiceClient
    {
        private readonly string _baseUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AvailabilityServiceClient(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _baseUrl = configuration["SlotService:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AvailavilityServiceResponse> GetWeeklyAvailableSlots(DateOnly date)
        {
            try
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

                if (string.IsNullOrEmpty(authHeader))
                {
                    throw new UnauthorizedAccessException("Authorization header is missing or invalid.");
                }

                var url = $"{_baseUrl}/GetWeeklyAvailability/{date:yyyyMMdd}";
                var responseString = await url
                    .WithHeader("Authorization", authHeader)
                    .GetStringAsync();

                return JsonSerializer.Deserialize<AvailavilityServiceResponse>(responseString)!;
            }
            catch (FlurlHttpException ex)
            {
                switch (ex.StatusCode)
                {
                    case (int)HttpStatusCode.BadRequest:
                        var errorMessage = await ex.GetResponseStringAsync();
                        if (errorMessage.Contains("datetime must be a Monday"))
                        {
                            throw new AppException("Bad Request: Datetime must be a Monday.", 400, ex);
                        }
                        throw new AppException("Bad Request: The request was invalid.", 400, ex);

                    case (int)HttpStatusCode.Unauthorized:
                        throw new AppException("Unauthorized: Authentication failed.", 401, ex);

                    case (int)HttpStatusCode.NotFound:
                        throw new AppException("Not Found: The requested resource could not be found.", 404, ex);

                    case (int)HttpStatusCode.InternalServerError:
                        throw new AppException("Internal Server Error: There was a problem with the server.", 500, ex);

                    default:
                        throw new AppException("An unexpected error occurred.", 500, ex);
                }
            }
            catch (JsonException ex)
            {
                throw new AppException("Invalid JSON response format.", 400, ex);
            }
            catch (Exception ex)
            {
                throw new AppException("An unexpected error occurred.", 500, ex);
            }
        }


        public async Task<bool> TakeSlotAsync(SlotDTO slot)
        {
            try
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

                if (string.IsNullOrEmpty(authHeader))
                {
                    throw new UnauthorizedAccessException("Authorization header is missing or invalid.");
                }

                var url = $"{_baseUrl}/TakeSlot";

                var response = await url
                    .WithHeader("Authorization", authHeader)
                    .PostJsonAsync(slot);

                if ((int)response.StatusCode == (int)HttpStatusCode.OK)
                {
                    return true; 
                }

                return false; 
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Bad Request: The request was invalid.", ex);
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Unauthorized: Authentication failed.", ex);
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new HttpRequestException("Not Found: The requested resource could not be found.", ex);
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException("Internal Server Error: There was a problem with the server.", ex);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("An unexpected error occurred.", ex);
            }
        }
    }
}
