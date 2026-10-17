namespace SmartStudent.Core.Exceptions;

/// <summary>
/// Exception thrown when authentication fails (invalid email, wrong password, or wrong role domain) (HTTP 401).
/// Always returns a generic message to prevent user and role enumeration attacks.
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Wrong email or password.")
        : base(message, 401)
    {
    }
}
