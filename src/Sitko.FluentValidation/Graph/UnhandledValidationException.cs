namespace Sitko.FluentValidation.Graph;

internal class UnhandledValidationException : Exception
{
    public UnhandledValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
