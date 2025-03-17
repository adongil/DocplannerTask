using Docplanner.Domain.AvailavilityService;
using Docplanner.Domain.DTO.Request;
using Docplanner.Infrastructure.Client;
using Docplanner.Infrastructure.Exceptions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

public class AvailabilityServiceClient : IAvailabilityServiceClient
{
    private readonly string _baseUrl;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AvailabilityServiceClient> _logger;

    public AvailabilityServiceClient(
        IConfiguration configuration, 
        IHttpContextAccessor httpContextAccessor,
        ILogger<AvailabilityServiceClient> logger)
    {
        _baseUrl = configuration["SlotService:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration));
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<AvailavilityServiceResponse> GetWeeklyAvailableSlots(DateOnly date)
    {
        try
        {
            _logger.LogInformation("Getting weekly availability for {Date}", date);

            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                throw new AppException("Authorization header is missing or invalid.", 401);
            }

            var url = $"{_baseUrl}/GetWeeklyAvailability/{date:yyyyMMdd}";
            var responseString = await url
                .WithHeader("Authorization", authHeader)
                .GetStringAsync();

            _logger.LogInformation("Weekly availability retrieved successfully for {Date} and response {Response}", date, responseString);

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
            _logger.LogInformation("Taking slot for {Slot}", slot);

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
                _logger.LogInformation("Slot taken successfully for {Slot}", slot);

                return true; 
            }

            _logger.LogWarning("Failed to take slot for {Slot}", slot);

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
