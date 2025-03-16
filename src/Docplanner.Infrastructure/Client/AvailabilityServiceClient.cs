﻿using System.Net;
using System.Text.Json;
using Flurl.Http;
using Docplanner.Domain.AvailavilityService;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

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
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                var errorMessage = await ex.GetResponseStringAsync();
                if (errorMessage.Contains("datetime must be a Monday"))
                {
                    throw new HttpRequestException("Bad Request: Datetime must be a Monday.", ex);
                }
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
            catch (JsonException ex)
            {
                throw new HttpRequestException("Invalid JSON response format.", ex);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("An unexpected error occurred.", ex);
            }
        }
    }
}
