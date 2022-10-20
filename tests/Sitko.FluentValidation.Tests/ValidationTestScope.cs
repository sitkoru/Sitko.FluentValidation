using Sitko.Core.App;

namespace Sitko.FluentValidation.Tests;

using Core.Xunit;
using global::FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

[UsedImplicitly]
public class ValidationTestScope : BaseTestScope
{
    protected override IServiceCollection ConfigureServices(IApplicationContext applicationContext,
        IServiceCollection services, string name)
    {
        base.ConfigureServices(applicationContext, services, name);
        services.AddFluentValidationExtensions();
        services.AddValidatorsFromAssemblyContaining<FluentGraphValidatorTests>();
        return services;
    }
}
