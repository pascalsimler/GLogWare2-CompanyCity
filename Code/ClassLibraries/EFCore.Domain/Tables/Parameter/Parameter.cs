namespace Gudel.GLogWare.EFCore.Domain;

public class Parameter : BaseTracking
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsDisplayed { get; set; } = true;
    public string? DicoRef { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public string? Value { get; set; }

    public ParameterType ParameterTypeRecord { get; set; } = null!;
}
