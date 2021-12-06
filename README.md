# Sitko.FluentValidation

![Nuget](https://img.shields.io/nuget/dt/Sitko.FluentValidation) ![Nuget](https://img.shields.io/nuget/v/Sitko.FluentValidation)

Set of extensions for [FluentValidation](https://fluentvalidation.net/) library

# Installation

```
dotnet add package Sitko.FluentValidation
```

Register in DI in `Startup.cs` or `Program.cs`

```c#
services.AddFluentValidationExtensions();
```

# FluentGraphValidator

FluentGraphValidator allows to recursively validate model graph.

```c#
public class MyService {
    
    private FluentGraphValidator _fluentGraphValidator;

    public MyService(FluentGraphValidator fluentGraphValidator) {
        _fluentGraphValidator = fluentGraphValidator;
    }
    
    public async Task SaveDataAsync(MyData data, CancellationToken cancellationToken = default) {
        var result = await _fluentGraphValidator.TryValidateModelAsync(data, cancellationToken);
        if(result.IsValid) {
            // validation was successfull
        }
        else {
            // every model validation results is in result.Results
        }
    }
}

public class MyData {
    public string Name { get; set; } = "";
    public MySubData? SubData { get; set; }
    public List<MySubData> SubDataList { get; set; } = new List<MySubData>();
}

public class MySubData {
    public string SubName { get; set; } = "";
}

public class MyDataValidator : AbstractValidator<MyData> {
    public MyDataValidator() {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class MySubDataValidator : AbstractValidator<MySubData> {
    public MySubDataValidator() {
        RuleFor(x => x.SubName).NotEmpty();
    }
}

```
