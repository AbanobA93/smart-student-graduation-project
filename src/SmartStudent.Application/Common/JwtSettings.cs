namespace SmartStudent.Application.Common;

/// <summary>
/// Strongly-typed configuration settings for JWT token generation and validation.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
