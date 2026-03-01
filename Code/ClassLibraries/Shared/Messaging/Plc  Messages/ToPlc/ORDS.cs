namespace Gudel.GLogWare.Shared;

public class ORDS
{
    public string Jobid { get; set; } = string.Empty;
    public ORDSPosition PickPosition { get; set; } = new ORDSPosition();
    public ORDSPosition DropPosition { get; set; } = new ORDSPosition();
}

public enum ORDSPositionType
{
    Undefined = 0,
    Conveyor = 1,
    Store = 2,
    Pallet = 3,
}

public class ORDSPosition
{
    public ORDSPositionType PositionType { get; set; }
    public string ConveyorPlace { get; set; } = string.Empty;
    public int XCell { get; set; }
    public int YCell { get; set; }
    public int XPosition { get; set; }
    public int YPosition { get; set; }
    public int ZOffset { get; set; }
}