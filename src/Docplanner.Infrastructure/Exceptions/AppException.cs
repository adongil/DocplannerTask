namespace Docplanner.Infrastructure.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode, Exception innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
