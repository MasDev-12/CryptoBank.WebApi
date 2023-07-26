namespace CryptoBank.WebApi.Features.Deposits.Errors
{
    public static class DepositValidationErrors
    {
        public static string Prefix = "deposits_validation_";

        public static string CurrencyCodeRequired = Prefix + "currency_code_required";
        public static string CurrencyCodeLenght = Prefix + "currency_code_max_lenght_three";
    }
}
