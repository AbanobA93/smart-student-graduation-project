using System.Security.Claims;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.API.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetProfileId(this ClaimsPrincipal principal)
    {
        var claimValue = principal.FindFirstValue("profileId");
        if (string.IsNullOrWhiteSpace(claimValue) || !Guid.TryParse(claimValue, out var profileId))
        {
            throw new ForbiddenAccessException("Valid domain profile claim was not found in the token.");
        }

        return profileId;
    }
}
