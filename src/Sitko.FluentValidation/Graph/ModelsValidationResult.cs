namespace Sitko.FluentValidation.Graph;

public class ModelsValidationResult
{
    public bool IsValid => Results.All(r => r.IsValid);
    public HashSet<ModelValidationResult> Results { get; } = new();
}
