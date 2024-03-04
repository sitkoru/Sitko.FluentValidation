using Microsoft.Extensions.Hosting;
using Sitko.FluentValidation.Graph;

namespace Sitko.FluentValidation.Tests;

using Core.Xunit;
using global::FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

[UsedImplicitly]
public class ValidationTestScope : BaseTestScope
{
    protected override IHostApplicationBuilder ConfigureServices(IHostApplicationBuilder builder, string name)
    {
        base.ConfigureServices(builder, name);
        builder.Services.AddFluentValidationExtensions();
        builder.Services.AddValidatorsFromAssemblyContaining<FluentGraphValidatorTests>();
        return builder;
    }
}

public class ValidationWithExcludedPrefixTestScope : ValidationTestScope
{
    protected override IHostApplicationBuilder ConfigureServices(IHostApplicationBuilder builder, string name)
    {
        base.ConfigureServices(builder, name);
        builder.Services.Configure<FluentGraphValidatorOptions>(options => options.NamespacePrefixes.Add("Sitko"));
        return builder;
    }
}
