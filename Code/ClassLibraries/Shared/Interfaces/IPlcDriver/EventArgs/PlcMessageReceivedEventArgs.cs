namespace Gudel.GLogWare.Shared;

public class PlcMessageReceivedEventArgs : EventArgs
{
    public PlcMessage plcMessage { get; set; } = null!;
}
