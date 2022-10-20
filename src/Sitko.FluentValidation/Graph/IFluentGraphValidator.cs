namespace Sitko.FluentValidation.Graph;

public interface IFluentGraphValidator
{
    Task<ModelsValidationResult> TryValidateFieldAsync(object model, string fieldName,
        CancellationToken cancellationToken = default);

    Task<ModelsValidationResult> TryValidateModelAsync(object model,
        CancellationToken cancellationToken = default);
}
