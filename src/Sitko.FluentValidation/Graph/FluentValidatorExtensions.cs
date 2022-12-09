using FluentValidation;

namespace Sitko.FluentValidation.Graph;

// ReSharper disable once UnusedType.Global
public static class FluentValidatorExtensions
{
    // ReSharper disable once MemberCanBePrivate.Global
    public static ValidationContext<object>? GetParentContext(this IValidationContext validationContext) =>
        validationContext is { IsChildContext: true, ParentContext: ValidationContext<object> objectValidationContext }
            ? objectValidationContext
            : null;

    public static object? GetParentInstance(this IValidationContext validationContext) =>
        GetParentContext(validationContext)?.InstanceToValidate;
}
