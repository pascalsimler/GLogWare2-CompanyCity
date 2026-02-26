namespace Gudel.GLogWare.Shared;

public class PlcMessage
{
    PlcMessageIdentifiers Identifier { get; set; }
    public object? Data { get; set; }
}
