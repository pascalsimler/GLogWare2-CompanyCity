namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public  class ParameterType: ISeedData<ParameterType>
{
    public string Identifier { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Parameter> Parameters { get; set; } = new List<Parameter>();

    public static IEnumerable<ParameterType> SeedData()
    {
        return new List<ParameterType>() {
            new ParameterType {
                Identifier = nameof(ParameterTypeIdentifiers.STRING),
                Description = "NVARCHAR"
            },
            new ParameterType {
                Identifier = nameof(ParameterTypeIdentifiers.NUMBER),
                Description = "NUMBER, INT, DECIMAL, DOUBLE"
            },
            new ParameterType {
                Identifier = nameof(ParameterTypeIdentifiers.BOOL),
                Description = "BIT"
            },
            new ParameterType {
                Identifier = nameof(ParameterTypeIdentifiers.DATE),
                Description = "DATE"
            },
            new ParameterType {
                Identifier = nameof(ParameterTypeIdentifiers.TIME),
                Description = "TIME"
            },
            new ParameterType {
                Identifier = nameof(ParameterTypeIdentifiers.DATETIME),
                Description = "DATETIME"
            },
        };
    }
}
