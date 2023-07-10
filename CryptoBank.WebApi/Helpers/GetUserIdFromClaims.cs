using CryptoBank.WebApi.Errors.Exceptions;
using System.Security.Claims;

namespace CryptoBank.WebApi.Helpers;

public static class GetUserIdFromClaims
{
    public static long GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier)!.Value;
        if (long.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new InternalErrorException("Internal error exception");
    }
}
