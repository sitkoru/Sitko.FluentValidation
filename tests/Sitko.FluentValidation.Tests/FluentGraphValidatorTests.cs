namespace Sitko.FluentValidation.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Xunit;
using Data;
using FluentAssertions;
using Graph;
using Xunit;
using Xunit.Abstractions;

public class FluentGraphValidatorTests : BaseTest<ValidationTestScope>
{
    public FluentGraphValidatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task ValidateParent()
    {
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        var foo = new FooModel();
        var result = await validator.TryValidateModelAsync(foo);
        result.IsValid.Should().BeFalse();
        result.Results.Should().ContainSingle();
        var fooResult = result.Results.First();
        fooResult.Model.Should().Be(foo);
        fooResult.Errors.Should().HaveCount(2);
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.Id));
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(FooModel.BarModels));
    }

    [Fact]
    public async Task ValidateChild()
    {
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        var bar = new BarModel();
        var foo = new FooModel { Id = Guid.NewGuid(), BarModels = new List<BarModel> { bar } };
        var result = await validator.TryValidateModelAsync(foo);
        result.IsValid.Should().BeFalse();
        result.Results.Should().HaveCount(2);
        result.Results.Where(r => r.IsValid).Should().ContainSingle();
        var fooResult = result.Results.First(r => !r.IsValid);
        fooResult.Model.Should().Be(bar);
        fooResult.Errors.Should().HaveCount(1);
        fooResult.Errors.Should().Contain(failure => failure.PropertyName == nameof(BarModel.TestGuid));
    }

    [Fact]
    public async Task ValidateOnlyChildren()
    {
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        var fooBar = new FooBarModel();
        var result = await validator.TryValidateModelAsync(fooBar);
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
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        var fooBar = new FooBarModel();
        var result = await validator.TryValidateFieldAsync(fooBar, nameof(FooBarModel.Foo));
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
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        var model = "some string";
        var result = await validator.TryValidateModelAsync(model);
        result.IsValid.Should().BeTrue();

        var enumValue = BarType.Baz;
        result = await validator.TryValidateModelAsync(enumValue);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task NoValidator()
    {
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        var model = new BazModel();
        var result = await validator.TryValidateModelAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNotValidate()
    {
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        // ReSharper disable once ExplicitCallerInfoArgument
        var scopeWithExcludedPrefix = await GetScopeAsync<ValidationWithExcludedPrefixTestScope>("scopeWithExcludedPrefix");
        var validatorWithExcludedPrefix = scopeWithExcludedPrefix.GetService<FluentGraphValidator>();
        var foo = new FooModel();
        var result = await validator.TryValidateModelAsync(foo);
        result.IsValid.Should().BeFalse();
        var resultWithExcludedPrefix = await validatorWithExcludedPrefix.TryValidateModelAsync(foo);
        resultWithExcludedPrefix.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ResultToString()
    {
        var scope = await GetScopeAsync();
        var validator = scope.GetService<FluentGraphValidator>();
        var foo = new FooModel();
        var result = await validator.TryValidateModelAsync(foo);
        result.ToString().Should()
            .Be(
                "Validation errors: \nModel Sitko.FluentValidation.Tests.Data.FooModel\n\tId: 'Id' must not be empty.\n\tBarModels: 'Bar Models' must not be empty.");
    }
}
