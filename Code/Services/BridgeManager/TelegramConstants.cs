namespace Gudel.GLogWare.BridgeManager;


/// <summary>
/// Constants for the communication GLogWare-PLC
/// </summary>
public struct TelegramConstants
{
    /// <summary>
    /// Start of Text ASCII code
    /// </summary>
    public const byte STX = 0x02;

    /// <summary>
    /// End of Text ASCII Code
    /// </summary>
    public const byte ETX = 0x03;

    /// <summary>
    /// Fixed dize telegram length for the GLogWare-PLC data exchange
    /// </summary>
    public const int TELEGRAM_LENGTH = 240;

    /// <summary>
    /// GLogWare sender/receiver identifier
    /// </summary>
    public const string GLOGWARE_IDENTIFIER = "GLOGWARE";
}