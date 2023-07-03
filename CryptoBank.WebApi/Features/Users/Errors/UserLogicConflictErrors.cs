namespace CryptoBank.WebApi.Features.Users.Errors;

public static class UserLogicConflictErrors
{
    private const string Prefix = "Users_";

    public static string RoleAlreadyUse = Prefix + "role_already_added_previous";
}

