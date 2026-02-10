namespace IndasEstimo.Application.Exceptions;

public class UnauthorizedTenantAccessException : Exception
{
    public UnauthorizedTenantAccessException(string message) : base(message)
    {
    }

    public UnauthorizedTenantAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
