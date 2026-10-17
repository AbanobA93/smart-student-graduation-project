namespace SmartStudent.Core.Exceptions;

/// <summary>
/// Exception thrown when a requested entity or resource is not found (HTTP 404).
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message, 404)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with ID '{key}' was not found.", 404)
    {
    }
}
