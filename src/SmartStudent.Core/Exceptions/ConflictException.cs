namespace SmartStudent.Core.Exceptions;

/// <summary>
/// Exception thrown when an operation conflicts with existing state, such as duplicate enrollment or attendance (HTTP 409).
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(message, 409)
    {
    }
}
