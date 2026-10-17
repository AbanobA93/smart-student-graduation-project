namespace SmartStudent.Core.Exceptions;

/// <summary>
/// Base exception for all domain and business rule violations.
/// </summary>
public abstract class DomainException : Exception
{
    public int StatusCode { get; }

    protected DomainException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
