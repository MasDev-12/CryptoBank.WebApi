namespace CryptoBank.WebApi.Features.Accounts.Errors;

public class AccountValidationErrors
{
    private const string Prefix = "account_validation_";

    public const string CurrencyRequired = Prefix + "currency_required";

    public const string AccountExists = Prefix + "account_exists";

    public const string StartPeriod = Prefix + "start_period_required";
    public const string EndPeriod = Prefix + "end_period_required";
    public const string InvalidRange = Prefix + "invalid_range";
    public const string InvalidCurrency = Prefix + "invalid_currency";
}
