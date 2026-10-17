namespace SmartStudent.Core.Exceptions;

/// <summary>
/// Exception thrown when a user attempts an action on a resource they do not own or lack permission for (HTTP 403).
/// </summary>
public class ForbiddenAccessException : DomainException
{
    public ForbiddenAccessException(string message = "You do not have permission to perform this action on this resource.")
        : base(message, 403)
    {
    }
}
