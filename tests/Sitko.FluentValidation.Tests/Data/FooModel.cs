namespace Sitko.FluentValidation.Tests.Data;

using System;
using System.Collections.Generic;

public class FooModel
{
    public Guid Id { get; set; } = Guid.Empty;
    public List<BarModel> BarModels { get; set; } = new();
}
