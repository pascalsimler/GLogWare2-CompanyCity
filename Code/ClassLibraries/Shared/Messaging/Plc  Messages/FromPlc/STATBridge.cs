namespace Gudel.GLogWare.Shared;

public class STATBridge
{
    public string Bridge  { get; set; } = string.Empty;
    public STATBridgeWorkingModes WorkingMode { get; set; }
    public bool Parked { get; set; }
    public bool GripperOccupied { get; set; }
    public bool ErrorFlag { get; set; }
}

public enum STATBridgeWorkingModes
{
    AUTOMATIC,
    MANUAL,
    STOPPED,
}