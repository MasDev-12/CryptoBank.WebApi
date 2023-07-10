namespace CryptoBank.WebApi.Features.Auth.Errors;

public static class AuthValidationErrors
{
    public static string Prefix = "auth_validation_";

    public static string InvalidCredential = Prefix + "invalid_credential";

    public static string TokenRequired = Prefix + "token_required";
    public static string TokenNotExists = Prefix + "token_no_exists";
    public static string TokenInvalid = Prefix + "token_invalid";
}
