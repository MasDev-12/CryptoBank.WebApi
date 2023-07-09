namespace CryptoBank.WebApi.Features.Auth.Errors;

public static class AuthLogicConflictErrors
{
    private const string Prefix = "auth_";

    public static string AuthInvalidCredentials = Prefix + "invalid_credentials";
    public static string RefreshTokenError = Prefix + "refresh_token_service_problem";
}
