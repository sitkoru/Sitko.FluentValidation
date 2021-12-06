namespace Sitko.FluentValidation.Tests.Data;

using global::FluentValidation;
using JetBrains.Annotations;

[UsedImplicitly]
public class FooModelValidator : AbstractValidator<FooModel>
{
    public FooModelValidator()
    {
        RuleFor(f => f.Id).NotEmpty();
        RuleFor(f => f.BarModels).NotEmpty();
    }
}
