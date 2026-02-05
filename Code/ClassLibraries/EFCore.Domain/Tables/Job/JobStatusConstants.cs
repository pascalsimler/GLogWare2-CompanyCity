namespace Gudel.GLogWare.EFCore.Domain;

public static class JobStatusConstants
{
    public const string OkBridge = "OK_BRIDGE";
    public const string BridgeLoad = "BRIDGE_LOAD";
    public const string BridgeLoadEnd = "BRIDGE_LOAD_END";

    public const string WaitOnJobManager = "WAIT_ON_JOBMNG";
    public const string WaitOnRoute = "WAIT_ON_ROUTE";
    public const string ConveyorMove = "CONV_MOVE";
}