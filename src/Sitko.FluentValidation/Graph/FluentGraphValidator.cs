using System.Collections;
using System.Collections.Concurrent;
using FluentValidation;
using FluentValidation.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sitko.FluentValidation.Graph;

public partial class FluentGraphValidator
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

    private IValidationContext CreateValidationContext(object model, IValidatorSelector? validatorSelector = null)
    {
        validatorSelector ??= ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory();

        var context = new ValidationContext<object>(model, new PropertyChain(), validatorSelector)
        {
            RootContextData = { ["_FV_ServiceProvider"] = serviceScope.ServiceProvider }
        };

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
            ValidatorIsNotRegistered(logger, model.GetType().FullName!);
            TypesValidators[model.GetType()] = null;
        }
        else
        {
            TypesValidators[model.GetType()] = formValidatorType;
        }

        return validator;
    }

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Warning,
        Message = "FluentValidation.IValidator<{ModelType}> is not registered in the application service provider")]
    public static partial void ValidatorIsNotRegistered(ILogger logger, string modelType);

    public Task<ModelsValidationResult> TryValidateFieldAsync(object model, string fieldName,
        CancellationToken cancellationToken = default) =>
        TryValidateFieldAsync(model, fieldName, null, cancellationToken);

    public async Task<ModelsValidationResult> TryValidateFieldAsync(object model, string fieldName,
        ModelsValidationResult? result,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validatorSelector = new MemberNameValidatorSelector(new[] { fieldName });
            var validationContext = CreateValidationContext(model, validatorSelector);
            return await TryValidateModelAsync(model, validationContext, result, cancellationToken);
        }
        catch (Exception ex)
        {
            var msg = $"An unhandled exception occurred when validating field name: '{fieldName}'";

            msg += $" of model of type: '{model.GetType()}'";
            throw new UnhandledValidationException(msg, ex);
        }
    }

    public Task<ModelsValidationResult> TryValidateModelAsync(object model,
        CancellationToken cancellationToken = default) => TryValidateModelAsync(model, null, null, cancellationToken);

    public async Task<ModelsValidationResult> TryValidateModelAsync(object model,
        IValidationContext? validationContext, ModelsValidationResult? result,
        CancellationToken cancellationToken = default)
    {
        result ??= new ModelsValidationResult();
        if (model is null or string or int or double or float or bool or decimal or long or byte
                or char or uint or ulong or short or sbyte ||
            model.GetType().IsEnum ||
            model.GetType().Module.ScopeName == "CommonLanguageRuntimeLibrary" ||
            model.GetType().Module.ScopeName.StartsWith("System", StringComparison.InvariantCulture) ||
            model.GetType().Namespace?.StartsWith("System", StringComparison.InvariantCulture) == true ||
            model.GetType().Namespace?.StartsWith("Microsoft", StringComparison.InvariantCulture) == true
            || options.Value.NamespacePrefixes.Any(prefix =>
                model.GetType().Namespace?.StartsWith(prefix, StringComparison.InvariantCulture) == true))
        {
            return result;
        }

        try
        {
            validationContext ??= CreateValidationContext(model);

            if (result.Results.Any(r => r.Model.Equals(model)))
            {
                return result;
            }

            var modelResult = new ModelValidationResult(model);
            result.Results.Add(modelResult);

            var validator = TryGetModelValidator(model);
            if (validator is not null)
            {
                var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);
                if (!validationResult.IsValid)
                {
                    modelResult.Errors.AddRange(validationResult.Errors);
                }
            }

            foreach (var property in model.GetType().GetProperties())
            {
                var propertyModel = property.GetValue(model);


                if (propertyModel is not string && propertyModel is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        await TryValidateModelAsync(item, null, result, cancellationToken);
                    }
                }
                else if (propertyModel is not null)
                {
                    await TryValidateModelAsync(propertyModel, null, result, cancellationToken);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            var msg = $"An unhandled exception occurred when validating object of type: '{model.GetType()}'";
            throw new UnhandledValidationException(msg, ex);
        }
    }
}
