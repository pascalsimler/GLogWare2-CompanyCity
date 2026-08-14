namespace Gudel.GLogWare.Entities;

public enum JobStatusIdentifiers
{
    /// <summary>
    /// Order is ready to be sent to the PLC.
    /// </summary>
    OK_BRIDGE,
    
    /// <summary>
    /// Bridge is loading.
    /// </summary>
    BRIDGE_LOAD,

    /// <summary>
    /// 
    /// </summary>
    BRIDGE_LOAD_END,

    /// <summary>
    /// 
    /// </summary>
    OK_BRIDGE_UNLOAD,
    
    /// <summary>
    /// 
    /// </summary>
    BRIDGE_UNLOAD,
    
    /// <summary>
    /// 
    /// </summary>
    BRIDGE_UNLOAD_END,

    /// <summary>
    /// 
    /// </summary>
    WAIT_ON_JOBMANAGER,
    
    /// <summary>
    /// 
    /// </summary>
    WAIT_ON_ROUTE,
    
    /// <summary>
    /// 
    /// </summary>
    CONVEYOR_MOVE,
}