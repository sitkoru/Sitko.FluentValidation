using System;
using System.Collections.Generic;

namespace Sitko.FluentValidation.Tests.Data;

public class BarModel
{
    public Guid TestGuid { get; set; } = Guid.Empty;
    public BarType Type { get; set; } = BarType.Bar;
    public int Val { get; set; }

    [SkipGraphValidation] public List<FooModel> FooModels { get; set; } = new();

    private readonly int someInt;

    public int WithoutGetter
    {
        init => someInt = value;
    }
}
