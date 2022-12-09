namespace Sitko.FluentValidation.Graph;

public interface IFluentGraphValidator
{
    Task<ModelsValidationResult> TryValidateFieldAsync(object model, string fieldName,
        CancellationToken cancellationToken = default);

    Task<ModelsValidationResult> TryValidateModelAsync(object model,
        CancellationToken cancellationToken = default);

    Task<ModelsValidationResult> TryValidateFieldAsync(ModelFieldGraphValidationContext fieldGraphValidationContext,
        CancellationToken cancellationToken = default);

    Task<ModelsValidationResult> TryValidateModelAsync(ModelGraphValidationContext modelGraphValidationContext,
        CancellationToken cancellationToken = default);
}
