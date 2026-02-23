namespace Gudel.GLogWare.BridgeDriver;


/// <summary>
/// Constants for the communication GLogWare-PLC
/// </summary>
public struct DriverConstants
{
    /// <summary>
    /// Start of Text ASCII code
    /// </summary>
    public const byte STX = (byte)'$';
    //public const byte STX = 0x02;

    /// <summary>
    /// End of Text ASCII Code
    /// </summary>
    public const byte ETX = (byte)'#';
    //public const byte ETX = 0x03;

    /// <summary>
    /// Fixed dize telegram length for the GLogWare-PLC data exchange
    /// </summary>
    public const int TELEGRAM_LENGTH = 32;
    //public const int TELEGRAM_LENGTH = 240;

    /// <summary>
    /// Valid names for received telegrams (used by the regex validation)
    /// </summary>
    public const string VALID_RECEIVING_NAMES = @"\b(STAT|ALRM|COMP|ACKN)\b";

    /// <summary>
    /// Valid names for sent telegrams (used by the regex validation)
    /// </summary>
    public const string VALID_SENDING_NAMES = @"\b(ORDS|ACKN)\b";

    /// <summary>
    /// GLogWare sender/receiver identifier
    /// </summary>
    public const string GLOGWARE_IDENTIFIER = "GLOGWARE";

    /// <summary>
    /// Telegram template
    /// </summary>
    public const string TELEGRAM_TEMPLATE = "[STX][AckFlag][Counter][Receiver][Sender][Name][Data][ETX]";
                                          //    1 +    1   +    1   +    8    +    8  +   4 + 216 +  1   = 240 bytes   
}
