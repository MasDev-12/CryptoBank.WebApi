using CryptoBank.WebApi.Errors.Exceptions.Base;

namespace CryptoBank.WebApi.Errors.Exceptions;

public class InternalErrorException : ErrorException
{
    public InternalErrorException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
