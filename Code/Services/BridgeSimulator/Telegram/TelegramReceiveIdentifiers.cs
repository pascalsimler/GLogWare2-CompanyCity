namespace Gudel.GLogWare.BridgeSimulator;

/// <summary>
/// All valid telegram names Plc sends to GLogWare
/// </summary>
public enum TelegramReceiveIdentifiers
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

