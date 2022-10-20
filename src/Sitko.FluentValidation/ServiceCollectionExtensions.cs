using Microsoft.Extensions.Configuration;

namespace Sitko.FluentValidation;

using Graph;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentValidationExtensions(this IServiceCollection serviceCollection) =>
        serviceCollection.AddFluentGraphValidator();

    public static IServiceCollection AddFluentValidationExtensions(this IServiceCollection serviceCollection,
        Action<FluentGraphValidatorOptions> configure) => serviceCollection.AddFluentGraphValidator(configure);

    public static IServiceCollection AddFluentValidationExtensions(this IServiceCollection serviceCollection,
        Action<FluentGraphValidatorOptions> configure, string configurationSection) =>
        serviceCollection.AddFluentGraphValidator(configure, configurationSection);

    public static IServiceCollection AddFluentGraphValidator(this IServiceCollection serviceCollection,
        Action<FluentGraphValidatorOptions>? configure = null, string configurationSection = "FluentGraphValidator")
    {
        serviceCollection.AddScoped<FluentGraphValidator>();
        serviceCollection.AddOptions<FluentGraphValidatorOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection(configurationSection).Bind(options);
            })
            .PostConfigure(
                options =>
                {
                    configure?.Invoke(options);
                });
        return serviceCollection;
    }
}
