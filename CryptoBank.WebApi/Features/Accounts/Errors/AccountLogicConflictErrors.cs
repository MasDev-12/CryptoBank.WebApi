namespace CryptoBank.WebApi.Features.Accounts.Errors;

public class AccountLogicConflictErrors
{
    private const string Prefix = "Account_logic_conflict";

    public const string ExceedAccounts = Prefix + "exceeding_the_number_of_accounts";
}
