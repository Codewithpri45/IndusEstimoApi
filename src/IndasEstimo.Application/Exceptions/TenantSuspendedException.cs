namespace IndasEstimo.Application.Exceptions;

public class TenantSuspendedException : Exception
{
    public TenantSuspendedException(string message) : base(message)
    {
    }

    public TenantSuspendedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
