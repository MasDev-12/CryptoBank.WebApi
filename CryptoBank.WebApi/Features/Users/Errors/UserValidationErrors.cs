namespace CryptoBank.WebApi.Features.Users.Errors;

public class UserValidationErrors
{
    private const string Prefix = "users_validation_";

    public const string EmailRequired = Prefix + "email_required";
    public const string EmailInvalidFormat = Prefix + "email_invalid_format";
    public const string EmailExists = Prefix + "email_exists";

    public const string PasswordRequired = Prefix + "password_required";
    public const string PasswordLenght = Prefix + "password_short";

    public const string DateBirthRequired = Prefix + "date_birth_required";
    public const string AgeMoreTheEightTeen = Prefix + "age_less_then_eightTeen";

    public const string NotExists = Prefix + "not_exists";
    public const string RoleNotExists = Prefix + "role_not_exists";
    public const string InvalidCredentials = Prefix + "invalid_credentials";

    public const string UserIdRequired = Prefix + "userId_required";
    public const string InvalidRole = Prefix + "invalid_role_name";
}
