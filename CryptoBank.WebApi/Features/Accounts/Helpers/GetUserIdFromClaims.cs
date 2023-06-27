using System.Security.Claims;

namespace CryptoBank.WebApi.Features.Accounts.Helpers;

public static class GetUserIdFromClaims
{
    public static long GetUserId(ClaimsPrincipal user)
    {
        try
        {
            var userIdClaim = user.Claims.SingleOrDefault(i => i.Type == ClaimTypes.NameIdentifier)!.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new Exception();
        }
        catch (Exception)
        {
            throw new Exception();
        }
    }
}
