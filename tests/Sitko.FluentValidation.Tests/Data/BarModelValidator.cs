namespace Sitko.FluentValidation.Tests.Data;

using global::FluentValidation;
using JetBrains.Annotations;

[UsedImplicitly]
public class BarModelValidator : AbstractValidator<BarModel>
{
    public BarModelValidator() => RuleFor(b => b.TestGuid).NotEmpty();
}
