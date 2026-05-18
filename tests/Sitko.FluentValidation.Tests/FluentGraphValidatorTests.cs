using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sitko.FluentValidation.Graph;
using Sitko.FluentValidation.Tests.Data;
using Xunit;

namespace Sitko.FluentValidation.Tests;

public class FluentGraphValidatorTests
{
    [Fact]
    public async Task ValidateSkipAttribute()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var bar = new BarModel { Val = 0, FooModels = new List<FooModel> { new() { Id = Guid.NewGuid() } } };
        var foo = new FooModel { Id = Guid.NewGuid(), BarModels = new List<BarModel> { bar } };

        var result = await validator.TryValidateModelAsync(foo, cancellationToken);
        result.Results.Where(r => r.Model.GetType() == typeof(FooModel)).Should().ContainSingle();
    }

    [Fact]
    public async Task ValidateParent()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var foo = new FooModel();
        var result = await validator.TryValidateModelAsync(foo, cancellationToken);
        result.IsValid.Should().BeFalse();
        result.Results.Should().ContainSingle();
        var fooResult = result.Results.First();
        fooResult.Model.Should().Be(foo);
        fooResult.Errors.Should().HaveCount(2);
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.Id));
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.BarModels));
    }

    [Fact]
    public async Task ValidateParentOnAllSupportedTfms()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var foo = new FooModel();

        var result = await validator.TryValidateModelAsync(foo, cancellationToken);

        result.IsValid.Should().BeFalse();
        result.Results.Should().ContainSingle();
    }

    [Fact]
    public async Task ValidatorResolvedFromDiUsesSameScopeAsGraphValidator()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var graphValidator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var validator = scope.ServiceProvider.GetRequiredService<global::FluentValidation.IValidator<ScopedDependencyModel>>();
        var dependency = scope.ServiceProvider.GetRequiredService<ScopedDependency>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var model = new ScopedDependencyModel { ScopeId = dependency.Id };

        var directResult = await validator.ValidateAsync(model, cancellationToken);
        var graphResult = await graphValidator.TryValidateModelAsync(model, cancellationToken);

        directResult.IsValid.Should().BeTrue();
        graphResult.IsValid.Should().BeTrue();
        graphResult.Results.Should().ContainSingle();
    }

    [Fact]
    public async Task ValidateChild()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var bar = new BarModel { Val = 0 };
        var foo = new FooModel { Id = Guid.NewGuid(), BarModels = new List<BarModel> { bar } };
        var result = await validator.TryValidateModelAsync(foo, cancellationToken);
        result.IsValid.Should().BeFalse();
        result.Results.Should().HaveCount(2);
        result.Results.Where(r => r.IsValid).Should().ContainSingle();
        var fooResult = result.Results.First(r => !r.IsValid);
        fooResult.Model.Should().Be(bar);
        fooResult.Errors.Should().HaveCount(2);
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(BarModel.TestGuid));
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(BarModel.Val));
    }

    [Fact]
    public async Task ValidateBoth()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var bar = new BarModel { Val = 0 };
        var foo = new FooModel { Id = Guid.Empty, BarModels = new List<BarModel> { bar } };
        var result = await validator.TryValidateModelAsync(foo, cancellationToken);
        result.IsValid.Should().BeFalse();
        result.Results.Should().HaveCount(2);
        result.Results.Where(r => r.IsValid).Should().BeEmpty();
        result.Results.SelectMany(r => r.Errors).Should().HaveCount(2);
    }

    [Fact]
    public async Task SkipChildValidation()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var bar = new BarModel { Val = 0 };
        var foo = new FooModel { Id = Guid.NewGuid(), BarModels = new List<BarModel> { bar } };
        var result = await validator.TryValidateModelAsync(new ModelGraphValidationContext(foo,
            new GraphValidationContextOptions { NeedToValidate = model => model is not BarModel }), cancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateOnlyChildren()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var fooBar = new FooBarModel();
        var result = await validator.TryValidateModelAsync(fooBar, cancellationToken);
        result.IsValid.Should().BeFalse();
        result.Results.Should().HaveCount(2);
        result.Results.Where(r => r.IsValid).Should().ContainSingle();
        var fooResult = result.Results.First(r => !r.IsValid);
        fooResult.Model.Should().Be(fooBar.Foo);
        fooResult.Errors.Should().HaveCount(2);
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.Id));
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.BarModels));
    }

    [Fact]
    public async Task ValidateField()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var fooBar = new FooBarModel();
        var result = await validator.TryValidateFieldAsync(fooBar, nameof(FooBarModel.Foo), cancellationToken);
        result.IsValid.Should().BeFalse();
        result.Results.Should().HaveCount(2);
        result.Results.Where(r => r.IsValid).Should().ContainSingle();
        var fooResult = result.Results.First(r => !r.IsValid);
        fooResult.Model.Should().Be(fooBar.Foo);
        fooResult.Errors.Should().HaveCount(2);
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.Id));
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.BarModels));
    }

    [Fact]
    public async Task ValidateSystemType()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var model = "some string";
        var result = await validator.TryValidateModelAsync(model, cancellationToken);
        result.IsValid.Should().BeTrue();

        var enumValue = BarType.Baz;
        result = await validator.TryValidateModelAsync(enumValue, cancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task NoValidator()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var model = new BazModel();
        var result = await validator.TryValidateModelAsync(model, cancellationToken);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNotValidate()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        await using var scopeWithExcludedPrefix =
            await TestServiceScopeFactory.CreateAsync(options => options.NamespacePrefixes.Add("Sitko"));
        var validatorWithExcludedPrefix = scopeWithExcludedPrefix.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var foo = new FooModel();
        var result = await validator.TryValidateModelAsync(foo, cancellationToken);
        result.IsValid.Should().BeFalse();
        var resultWithExcludedPrefix = await validatorWithExcludedPrefix.TryValidateModelAsync(foo, cancellationToken);
        resultWithExcludedPrefix.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Path()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var foo = new FooModel { BarModels = new List<BarModel> { new() } };
        var result = await validator.TryValidateModelAsync(foo, cancellationToken);
        result.IsValid.Should().BeFalse();
        result.Results.First(validationResult => validationResult.Model == foo.BarModels.First()).Path.Should()
            .Be("BarModels.0.");
    }

    [Fact]
    public async Task ResultToString()
    {
        await using var scope = await TestServiceScopeFactory.CreateAsync();
        var validator = scope.ServiceProvider.GetRequiredService<FluentGraphValidator>();
        var cancellationToken = TestContext.Current.CancellationToken;
        var foo = new FooModel();
        var result = await validator.TryValidateModelAsync(foo, cancellationToken);
        result.ToString().Should()
            .Be(
                "Validation errors: \nModel Sitko.FluentValidation.Tests.Data.FooModel\n\tId: 'Id' must not be empty.\n\tBarModels: 'Bar Models' must not be empty.");
    }
}

public sealed class ScopedDependency
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class ScopedDependencyModel
{
    public Guid ScopeId { get; init; }
}

public sealed class ScopedDependencyModelValidator : AbstractValidator<ScopedDependencyModel>
{
    public ScopedDependencyModelValidator(ScopedDependency dependency)
    {
        RuleFor(model => model.ScopeId).Equal(dependency.Id);
    }
}
