using System;
using Sitko.FluentValidation.Graph;

namespace Sitko.FluentValidation.Tests.Data;

using global::FluentValidation;

public class BarModelValidator : AbstractValidator<BarModel>
{
    public BarModelValidator()
    {
        RuleFor(b => b.TestGuid).NotEmpty();
        RuleFor(model => model.Val).Equal(1).When((model, context) =>
        {
            if (context.GetParentInstance() is FooModel fooModel &&
                fooModel.Id != Guid.Empty)
            {
                return true;
            }

            return false;
        });
    }
}
