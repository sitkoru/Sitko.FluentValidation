using System;

namespace Sitko.FluentValidation.Tests.Data;

public class BarModel
{
    public Guid TestGuid { get; set; } = Guid.Empty;
    public BarType Type { get; set; } = BarType.Bar;
    public int Val { get; set; }
}
