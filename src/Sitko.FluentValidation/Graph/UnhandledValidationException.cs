namespace Sitko.FluentValidation.Graph;

internal sealed class UnhandledValidationException : Exception
{
    public UnhandledValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
