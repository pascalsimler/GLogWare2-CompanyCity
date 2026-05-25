namespace Gudel.GLogWare.Shared;

public class ORDS
{
    public string Jobid { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string Order { get; set; } = string.Empty;
    public ORDSPosition PickPosition { get; set; } = new ORDSPosition();
    public ORDSPosition DropPosition { get; set; } = new ORDSPosition();
    public int InnerDiameter { get; set; }
    public int OuterDiameter { get; set; }
    public int Width { get; set; }
    public int TireCount { get; set; }

    public ORDS()
    {
        PickPosition = new ORDSPosition();
        DropPosition = new ORDSPosition();
    }
}

public enum ORDSPositionTypes
{
    Undefined = 0,
    Conveyor = 1,
    Store = 2,
    Pallet = 3,
}

public class ORDSPosition
{
    public ORDSPositionTypes PositionType { get; set; } = ORDSPositionTypes.Undefined;
    public string ConveyorPlace { get; set; } = string.Empty;
    public int XCell { get; set; }
    public int YCell { get; set; }
    public int XPosition { get; set; }
    public int YPosition { get; set; }
    public int ZOffset { get; set; }
}