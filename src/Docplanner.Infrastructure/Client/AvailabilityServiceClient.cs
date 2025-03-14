using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Docplanner.Domain.AvailavilityService;
using System.Text.Json;
using System.Net;

namespace Docplanner.Infrastructure.Client
{
    public class AvailabilityServiceClient : IAvailabilityServiceClient
    {
        private readonly string _baseUrl;
        private readonly string _authHeader;

        public AvailabilityServiceClient(string AuthHeader)
        {
            _authHeader = AuthHeader;
            _baseUrl = "https://draliatest.azurewebsites.net/api/availability";
        }


        public async Task<AvailavilityServiceResponse> GetWeeklyAvailabilityAsync(DateOnly date)
        {
            try
            {
                var url = $"{_baseUrl}/GetWeeklyAvailability/{date:yyyyMMdd}";
                var responseString = await url
                    .WithHeader("Authorization", _authHeader)
                    .GetStringAsync();

                return JsonSerializer.Deserialize<AvailavilityServiceResponse>(responseString);
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                throw new Exception("Bad Request: The request was invalid.",ex);
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                throw new Exception("Unauthorized: Authentication failed.",ex);
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new Exception("Not Found: The requested resource could not be found.",ex);
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.InternalServerError)
            {
                throw new Exception("Internal Server Error: There was a problem with the server.",ex);
            }
            catch(Exception ex)
            {
                throw new Exception("An unexpected error occurred.", ex);
            }
        }
    }
}
