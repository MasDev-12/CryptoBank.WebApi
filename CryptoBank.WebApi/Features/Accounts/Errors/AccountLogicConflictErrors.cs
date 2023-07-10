namespace CryptoBank.WebApi.Features.Accounts.Errors;

public class AccountLogicConflictErrors
{
    private const string Prefix = "account_logic_conflict_";

    public const string ExceedAccounts = Prefix + "exceeding_the_number_of_accounts";
}
