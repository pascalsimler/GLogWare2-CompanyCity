namespace Gudel.GLogWare.BridgeSimulator;

/// <summary>
/// All valid telegram identifiers GLogWare sends to the PLC.
/// </summary>
public enum TelegramSendIdentifiers
{
    /// <summary>
    /// Bridge STATus (PLC->GLogWare)
    /// </summary>
    STAT,

    /// <summary>
    /// Bridge order COMPleted (PLC->GLogWare)
    /// </summary>
    COMP,

    /// <summary>
    /// ALaRM bitmask (PLC->GLogWare)
    /// </summary>
    ALRM,

    /// <summary>
    /// ACKNowledge telegram
    /// </summary>
    ACKN
}


