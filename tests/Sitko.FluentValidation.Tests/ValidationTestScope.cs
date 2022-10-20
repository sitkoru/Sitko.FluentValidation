using Sitko.Core.App;
using Sitko.FluentValidation.Graph;

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

public class ValidationWithExcludedPrefixTestScope : ValidationTestScope
{
    protected override IServiceCollection ConfigureServices(IApplicationContext applicationContext,
        IServiceCollection services, string name)
    {
        base.ConfigureServices(applicationContext, services, name);
        services.Configure<FluentGraphValidatorOptions>(options => options.NamespacePrefixes.Add("Sitko"));
        return services;
    }
}
