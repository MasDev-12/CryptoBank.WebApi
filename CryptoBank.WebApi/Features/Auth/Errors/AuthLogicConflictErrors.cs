namespace CryptoBank.WebApi.Features.Auth.Errors;

public static class AuthLogicConflictErrors
{
    private const string Prefix = "Auth_";

    public static string AuthInvalidCredentials = Prefix + "Invalid credentials";
    public static string RefreshTokenError = Prefix + "Refresh token service problem";
}
