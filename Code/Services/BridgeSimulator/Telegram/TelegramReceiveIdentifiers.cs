namespace Gudel.GLogWare.BridgeManager;

/// <summary>
/// All valid telegram names Plc sends to GLogWare
/// </summary>
public enum TelegramReceiveIdentifiers
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

