using System.Collections;
using System.Collections.Concurrent;
using FluentValidation;
using FluentValidation.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sitko.FluentValidation.Graph;

public class FluentGraphValidator : IFluentGraphValidator
{
    private static readonly ConcurrentDictionary<Type, Type?> TypesValidators = new();
    private readonly ILogger<FluentGraphValidator> logger;
    private readonly IOptions<FluentGraphValidatorOptions> options;
    private readonly IServiceScope serviceScope;

    public FluentGraphValidator(IServiceProvider serviceProvider, ILogger<FluentGraphValidator> logger,
        IOptions<FluentGraphValidatorOptions> options)
    {
        this.logger = logger;
        this.options = options;
        serviceScope = serviceProvider.CreateScope();
    }

    public Task<ModelsValidationResult> TryValidateFieldAsync(
        ModelFieldGraphValidationContext fieldGraphValidationContext,
        CancellationToken cancellationToken = default) =>
        TryValidateFieldAsync(fieldGraphValidationContext, null, null, cancellationToken);

    public Task<ModelsValidationResult> TryValidateModelAsync(ModelGraphValidationContext modelGraphValidationContext,
        CancellationToken cancellationToken = default) =>
        TryValidateModelAsync(modelGraphValidationContext,
            CreateValidationContext(modelGraphValidationContext.Model, null), null, "", cancellationToken);

    public Task<ModelsValidationResult> TryValidateFieldAsync(object model, string fieldName,
        CancellationToken cancellationToken = default) =>
        TryValidateFieldAsync(
            new ModelFieldGraphValidationContext(model, fieldName, new GraphValidationContextOptions()), null, null,
            cancellationToken);

    public Task<ModelsValidationResult> TryValidateModelAsync(object model,
        CancellationToken cancellationToken = default) =>
        TryValidateModelAsync(new ModelGraphValidationContext(model, new GraphValidationContextOptions()),
            CreateValidationContext(model, null), null, "", cancellationToken);

    private ValidationContext<object> CreateValidationContext(object model, ValidationContext<object>? parent,
        IValidatorSelector? validatorSelector = null)
    {
        validatorSelector ??= ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory();

        var context = parent?.CloneForChildValidator(model, true, validatorSelector) ??
                      new ValidationContext<object>(model, new PropertyChain(), validatorSelector) { RootContextData = { ["_FV_ServiceProvider"] = serviceScope.ServiceProvider } };

        return context;
    }

    private IValidator? TryGetModelValidator(object model)
    {
        if (TypesValidators.TryGetValue(model.GetType(), out var formValidatorType))
        {
            if (formValidatorType is null)
            {
                return null;
            }
        }
        else
        {
            var validatorType = typeof(IValidator<>);
            formValidatorType = validatorType.MakeGenericType(model.GetType());
        }

        var validator = serviceScope.ServiceProvider.GetService(formValidatorType) as IValidator;
        if (validator is null)
        {
            logger.LogWarning("FluentValidation.IValidator<{ModelType}> is not registered in the application service provider", model.GetType().FullName);
            TypesValidators[model.GetType()] = null;
        }
        else
        {
            TypesValidators[model.GetType()] = formValidatorType;
        }

        return validator;
    }

    private async Task<ModelsValidationResult> TryValidateFieldAsync(
        ModelFieldGraphValidationContext fieldGraphValidationContext,
        ModelsValidationResult? result,
        ValidationContext<object>? parentValidationContext = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validatorSelector = new MemberNameValidatorSelector(new[] { fieldGraphValidationContext.FieldName });
            var validationContext = CreateValidationContext(fieldGraphValidationContext.Model, parentValidationContext,
                validatorSelector);
            return await TryValidateModelAsync(fieldGraphValidationContext, validationContext, result, "",
                cancellationToken);
        }
        catch (Exception ex)
        {
            var msg =
                $"An unhandled exception occurred when validating field name: '{fieldGraphValidationContext.FieldName}'";

            msg += $" of model of type: '{fieldGraphValidationContext.Model.GetType()}'";
            throw new UnhandledValidationException(msg, ex);
        }
    }

    private async Task<ModelsValidationResult> TryValidateModelAsync(
        ModelGraphValidationContext modelGraphValidationContext,
        ValidationContext<object> validationContext, ModelsValidationResult? result,
        string path = "",
        CancellationToken cancellationToken = default)
    {
        result ??= new ModelsValidationResult();
        if (modelGraphValidationContext.Options.NeedToValidate?.Invoke(modelGraphValidationContext.Model) == false)
        {
            return result;
        }

        if (modelGraphValidationContext.Model is null or string or int or double or float or bool or decimal or long
                or byte
                or char or uint or ulong or short or sbyte ||
            modelGraphValidationContext.Model.GetType().IsEnum ||
            modelGraphValidationContext.Model.GetType().Module.ScopeName == "CommonLanguageRuntimeLibrary" ||
            modelGraphValidationContext.Model.GetType().Module.ScopeName
                .StartsWith("System", StringComparison.InvariantCulture) ||
            modelGraphValidationContext.Model.GetType().Namespace
                ?.StartsWith("System", StringComparison.InvariantCulture) == true ||
            modelGraphValidationContext.Model.GetType().Namespace
                ?.StartsWith("Microsoft", StringComparison.InvariantCulture) == true
            || options.Value.NamespacePrefixes.Any(prefix =>
                modelGraphValidationContext.Model.GetType().Namespace
                    ?.StartsWith(prefix, StringComparison.InvariantCulture) == true))
        {
            return result;
        }

        try
        {
            if (result.Results.Any(r => r.Model.Equals(modelGraphValidationContext.Model)))
            {
                return result;
            }

            var modelResult = new ModelValidationResult(modelGraphValidationContext.Model, path);
            result.Results.Add(modelResult);

            var validator = TryGetModelValidator(modelGraphValidationContext.Model);
            if (validator is not null)
            {
                var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);
                if (!validationResult.IsValid)
                {
                    foreach (var validationFailure in validationResult.Errors)
                    {
                        if (!result.Contains(validationFailure))
                        {
                            modelResult.Errors.Add(validationFailure);
                        }
                    }
                }
            }

            foreach (var property in modelGraphValidationContext.Model.GetType().GetProperties())
            {
                var propertyModel = property.GetValue(modelGraphValidationContext.Model);


                if (propertyModel is not string && propertyModel is IEnumerable enumerable)
                {
                    var i = 0;
                    foreach (var item in enumerable)
                    {
                        await TryValidateModelAsync(
                            new ModelGraphValidationContext(item, modelGraphValidationContext.Options),
                            CreateValidationContext(item, validationContext),
                            result,
                            path + property.Name + $".{i}.",
                            cancellationToken);
                        i++;
                    }
                }
                else if (propertyModel is not null)
                {
                    await TryValidateModelAsync(
                        new ModelGraphValidationContext(propertyModel, modelGraphValidationContext.Options),
                        CreateValidationContext(propertyModel, validationContext), result,
                        path + property.Name + ".",
                        cancellationToken);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            var msg =
                $"An unhandled exception occurred when validating object of type: '{modelGraphValidationContext.Model.GetType()}'";
            throw new UnhandledValidationException(msg, ex);
        }
    }
}

public record GraphValidationContextOptions
{
    public Func<object, bool>? NeedToValidate { get; init; }
}

public abstract record GraphValidationContext(GraphValidationContextOptions Options);

public record ModelGraphValidationContext
    (object Model, GraphValidationContextOptions Options) : GraphValidationContext(Options);

public record ModelFieldGraphValidationContext
    (object Model, string FieldName, GraphValidationContextOptions Options) : ModelGraphValidationContext(Model,
        Options);
