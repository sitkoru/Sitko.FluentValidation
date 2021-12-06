namespace Sitko.FluentValidation.Graph;

using System.Globalization;
using System.Text;

public class ModelsValidationResult
{
    public bool IsValid => Results.All(r => r.IsValid);
    public HashSet<ModelValidationResult> Results { get; } = new();

    public override string ToString()
    {
        var errors = new StringBuilder("Validation errors: ");
        foreach (var result in Results.Where(r => !r.IsValid))
        {
            errors.Append(
#if NET6_0_OR_GREATER
                CultureInfo.InvariantCulture,
#endif
                $"\n{result.ToString()}");
        }

        return errors.ToString();
    }
}
