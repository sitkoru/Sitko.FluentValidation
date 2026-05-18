using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitko.FluentValidation.Graph;

namespace Sitko.FluentValidation.Tests;

public static class TestServiceScopeFactory
{
    public static Task<TestServiceScope> CreateAsync(Action<FluentGraphValidatorOptions>? configureOptions = null)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddScoped<ScopedDependency>();
        builder.Services.AddFluentValidationExtensions();
        builder.Services.AddValidatorsFromAssemblyContaining<ScopedDependencyModelValidator>();
        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }

        var host = builder.Build();
        var scope = host.Services.CreateAsyncScope();
        return Task.FromResult(new TestServiceScope(host, scope));
    }
}

public sealed class TestServiceScope : IAsyncDisposable
{
    private readonly IHost host;
    private readonly AsyncServiceScope scope;

    public TestServiceScope(IHost host, AsyncServiceScope scope)
    {
        this.host = host;
        this.scope = scope;
    }

    public IServiceProvider ServiceProvider => scope.ServiceProvider;

    public async ValueTask DisposeAsync()
    {
        await scope.DisposeAsync();
        host.Dispose();
    }
}
