namespace Gudel.GLogWare.BridgeManager;

/// <summary>
/// All valid telegram identifiers GLogWare sends to the PLC.
/// </summary>
public enum TelegramSendIdentifiers
{
    /// <summary>
    /// LIFE signal (GLogWare -> PLC)
    /// </summary>
    LIFE,

    /// <summary>
    /// Bridge ORDer Sorter (GLogWare -> PLC)
    /// </summary>
    ORDS,

    /// <summary>
    /// Bridge order DELeTion (GLogWare -> PLC)
    /// </summary>
    DELT,

    /// <summary>
    /// ACKNowledge telegram
    /// </summary>
    ACKN
}


