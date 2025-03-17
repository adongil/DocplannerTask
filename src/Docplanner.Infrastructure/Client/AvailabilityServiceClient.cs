using Docplanner.Domain.AvailavilityService;
using Docplanner.Domain.DTO.Request;
using Docplanner.Infrastructure.Client;
using Docplanner.Infrastructure.Exceptions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json;

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
                throw new AppException("Authorization header is missing or invalid.", 401);
            }

            var url = $"{_baseUrl}/GetWeeklyAvailability/{date:yyyyMMdd}";
            var responseString = await url
                .WithHeader("Authorization", authHeader)
                .GetStringAsync();

            return JsonSerializer.Deserialize<AvailavilityServiceResponse>(responseString)!;
        }
        catch (FlurlHttpException ex)
        {
            if(ex.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                var errorResponse = await ex.GetResponseStringAsync();
                if (errorResponse.Contains("datetime must be a Monday"))
                {
                    throw new AppException("Datetime must be a Monday.", 400, ex);
                }
            }

            throw HandleFlurlException(ex);
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
                throw new AppException("Authorization header is missing or invalid.", 401);
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
        catch (FlurlHttpException ex)
        {
            throw HandleFlurlException(ex);
        }
        catch (Exception ex)
        {
            throw new AppException("An unexpected error occurred.", 500, ex);
        }
    }

    private AppException HandleFlurlException(FlurlHttpException ex)
    {
        switch (ex.StatusCode)
        {
            case (int)HttpStatusCode.BadRequest:
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
}
