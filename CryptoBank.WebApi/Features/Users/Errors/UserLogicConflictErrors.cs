namespace CryptoBank.WebApi.Features.Users.Errors;

public static class UserLogicConflictErrors
{
    private const string Prefix = "users_";

    public static string RoleAlreadyUse = Prefix + "role_already_used";
}

