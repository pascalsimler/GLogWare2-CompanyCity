namespace Gudel.GLogWare.Messages;

public class STATBridge
{
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