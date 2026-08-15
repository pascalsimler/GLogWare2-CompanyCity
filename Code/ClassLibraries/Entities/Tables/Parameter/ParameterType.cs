namespace Gudel.GLogWare.Entities;

[SeedOrder(1)]
public  class ParameterType: ISeedData<ParameterType>
{
    public string Identifier { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Parameter> Parameters { get; set; } = [];

    public static IEnumerable<ParameterType> SeedData()
    {
        return [
            new() {
                Identifier = nameof(ParameterTypeIdentifiers.STRING),
                Description = "NVARCHAR"
            },
            new() {
                Identifier = nameof(ParameterTypeIdentifiers.NUMBER),
                Description = "NUMBER, INT, DECIMAL, DOUBLE"
            },
            new() {
                Identifier = nameof(ParameterTypeIdentifiers.BOOL),
                Description = "BIT"
            },
            new() {
                Identifier = nameof(ParameterTypeIdentifiers.DATE),
                Description = "DATE"
            },
            new() {
                Identifier = nameof(ParameterTypeIdentifiers.TIME),
                Description = "TIME"
            },
            new() {
                Identifier = nameof(ParameterTypeIdentifiers.DATETIME),
                Description = "DATETIME"
            },
        ];
    }
}
