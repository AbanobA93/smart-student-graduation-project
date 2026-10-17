namespace SmartStudent.Core.Exceptions;

/// <summary>
/// Exception thrown when a business rule or input validation fails (HTTP 400).
/// </summary>
public class BadRequestException : DomainException
{
    public IReadOnlyList<string> Errors { get; }

    public BadRequestException(string message, IEnumerable<string>? errors = null)
        : base(message, 400)
    {
        Errors = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
    }
}
