namespace Sitko.FluentValidation.Tests;

using Core.Xunit;
using global::FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[UsedImplicitly]
public class ValidationTestScope : BaseTestScope
{
    protected override IServiceCollection ConfigureServices(IConfiguration configuration,
        IHostEnvironment environment,
        IServiceCollection services, string name)
    {
        base.ConfigureServices(configuration, environment, services, name);
        services.AddFluentValidationExtensions();
        services.AddValidatorsFromAssemblyContaining<FluentGraphValidatorTests>();
        return services;
    }
}
