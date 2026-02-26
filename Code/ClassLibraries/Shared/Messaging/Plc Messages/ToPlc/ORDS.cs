namespace Gudel.GLogWare.Shared;

public class ORDS
{
    public ORDSPosition PickPosition { get; set; } = null!;
    public ORDSPosition DropPosition { get; set; } = null!;
}


public class ORDSPosition
{
    public string XCell { get; set; } = null!;
    public string YCell { get; set; } = null!;
    public int XPos { get; set; }
    public int YPos { get; set; }
    public int ZOffset { get; set; }
}