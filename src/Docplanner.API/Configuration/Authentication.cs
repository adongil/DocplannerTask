using Docplanner.API.Auth;
using Microsoft.AspNetCore.Authentication;

namespace Docplanner.API.Configuration;

public static class Authentication
{
    public static void AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication("BasicAuth")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuth", options => { });

        services.AddAuthorization();  
    }
}
