namespace Gudel.GLogWare.EFCore.Domain;

public enum JobStatusIdentifiers
{
    OK_BRIDGE,
    BRIDGE_LOAD,
    BRIDGE_LOAD_END,
    OK_BRIDGE_UNLOAD,
    BRIDGE_UNLOAD,
    BRIDGE_UNLOAD_END,

    WAIT_ON_JOBMANAGER,
    WAIT_ON_ROUTE,
    CONVEYOR_MOVE,
}