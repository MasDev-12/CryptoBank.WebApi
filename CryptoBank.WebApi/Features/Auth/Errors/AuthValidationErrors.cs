namespace CryptoBank.WebApi.Features.Auth.Errors;

public static class AuthValidationErrors
{
    public static string Prefix = "Auth_validation_";

    public static string InvalidCredential = Prefix + "invalid credential";

    public static string TokenRequired = Prefix + "token_required";
    public static string TokenNotExist = Prefix + "token_no_exist";
    public static string TokenInvalid = Prefix + "token_invalid";
}
