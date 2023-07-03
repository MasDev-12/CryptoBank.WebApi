namespace CryptoBank.WebApi.Features.Users.Errors;

public class UserValidationErrors
{
    private const string Prefix = "Users_validation_";

    public const string EmailRequired = Prefix + "email_required";
    public const string EmailInvalidFormat = Prefix + "email_invalid_format";
    public const string EmailExistOrInvalid = Prefix + "email_exist_or_invalid";

    public const string PasswordRequired = Prefix + "password_required";
    public const string PasswordLenght = Prefix + "password_short";

    public const string DateBirthRequired = Prefix + "date_birth_required";
    public const string AgeMoreTheEightTeen = Prefix + "age_less_then_eightTeen";

    public const string NotExist = Prefix + "not_exist";
    public const string RoleNotExist = Prefix + "role_not_exist";
    public const string InvalidCredentials = Prefix + "invalid_credentials";

    public const string UserIdRequired = Prefix + "userId_required";
    public const string InvalidRole = Prefix + "invalid_role_name";
}
