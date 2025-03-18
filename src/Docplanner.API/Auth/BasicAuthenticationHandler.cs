using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;


namespace Docplanner.API.Auth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
public BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
    : base(options, logger, encoder, clock) { }

protected override Task<AuthenticateResult> HandleAuthenticateAsync()
{
    string authHeader = Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
    {
        return Task.FromResult(AuthenticateResult.Fail("No Authorization header or not Basic"));
    }

    string encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
    string plainCredentials;
    try
    {
        byte[] bytes = Convert.FromBase64String(encodedCredentials);
        plainCredentials = Encoding.UTF8.GetString(bytes);
    }
    catch
    {
        return Task.FromResult(AuthenticateResult.Fail("Invalid Base64 Authorization header"));
    }
    string[] parts = plainCredentials.Split(':', 2);
    if (parts.Length != 2)
    {
        return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credentials format"));
    }
    string username = parts[0];
    string password = parts[1];

    var claims = new[] { new Claim(ClaimTypes.Name, username) };
    var identity = new ClaimsIdentity(claims, Scheme.Name);
    var principal = new ClaimsPrincipal(identity);

    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
}
}