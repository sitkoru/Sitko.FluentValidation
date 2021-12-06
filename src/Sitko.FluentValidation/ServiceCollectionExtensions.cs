namespace Sitko.FluentValidation;

using Graph;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentValidationExtensions(this IServiceCollection serviceCollection) =>
        serviceCollection.AddFluentGraphValidator();

    public static IServiceCollection AddFluentGraphValidator(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<FluentGraphValidator>();
        return serviceCollection;
    }
}
