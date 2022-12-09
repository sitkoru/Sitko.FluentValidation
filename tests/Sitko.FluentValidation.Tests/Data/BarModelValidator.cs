using System;

namespace Sitko.FluentValidation.Tests.Data;

using global::FluentValidation;
using JetBrains.Annotations;

[UsedImplicitly]
public class BarModelValidator : AbstractValidator<BarModel>
{
    public BarModelValidator()
    {
        RuleFor(b => b.TestGuid).NotEmpty();
        RuleFor(model => model.Val).Equal(1).When((model, context) =>
        {
            if (context.IsChildContext &&
                ((IValidationContext)context).ParentContext.InstanceToValidate is FooModel fooModel &&
                fooModel.Id != Guid.Empty)
            {
                return true;
            }

            return false;
        });
    }
}
