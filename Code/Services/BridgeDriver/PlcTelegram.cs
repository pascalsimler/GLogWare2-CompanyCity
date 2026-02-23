using System.Text;
using System.Text.RegularExpressions;

namespace Gudel.GLogWare.BridgeDriver;

/// <summary>
/// PLC telegram representation
/// </summary>
public class PlcTelegram
{
    #region Injected members
    private readonly ILogger _logger;
    #endregion

    public PlcTelegram(ILogger logger)
    {
        _logger = logger;
        Bytes = new byte[DriverConstants.TELEGRAM_LENGTH];
    }

    /// <summary>
    /// byte array for buffering the received telegram so far
    /// </summary>
    public byte[] Bytes { get; set; }

    /// <summary>
    /// The complete content of the Plc telegram in a string
    /// </summary>
    public string Telegram { get; set; } = string.Empty;

    /// <summary>
    /// Acknowledge flag (0 or 1)
    /// </summary>
    public string AckFlag { get; set; } = string.Empty;

    /// <summary>
    /// Telegram counter (1 to 9)
    /// </summary>
    public string Counter { get; set; } = string.Empty;

    /// <summary>
    /// Receiver of the telegram
    /// </summary>
    public string Receiver { get; set; } = string.Empty;

    /// <summary>
    /// Sender of the telegram
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// The telegram name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The datas of the telegram
    /// </summary>
    public string Data{ get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public void Build()
    {
        Telegram = DriverConstants.TELEGRAM_TEMPLATE;
        Telegram = Telegram.Replace("[STX]", Convert.ToChar(DriverConstants.STX).ToString());
        Telegram = Telegram.Replace("[ETX]", Convert.ToChar(DriverConstants.ETX).ToString());
        Telegram = Telegram.Replace("[AckFlag]", AckFlag);
        Telegram = Telegram.Replace("[Counter]", Counter);
        Telegram = Telegram.Replace("[Receiver]", Receiver);
        Telegram = Telegram.Replace("[Sender]", Sender);
        Telegram = Telegram.Replace("[Name]", Name);
        Telegram = Telegram.Replace("[Data]", Data.PadRight(216, '.'));

        Array.Clear(Bytes, 0, Bytes.Length);
        byte[] tmpBuf = Encoding.ASCII.GetBytes(Telegram);
        Array.Copy(tmpBuf, 0, Bytes, 0, tmpBuf.Length);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Parse()
    {
   
    }

    public bool Validate()
    {
        byte b;

        Telegram = Encoding.Default.GetString(Bytes, 0, Bytes.Length);
        AckFlag = Telegram.Substring(1, 1);
        Counter = Telegram.Substring(2, 1);
        Receiver = Telegram.Substring(3, 8);
        Sender = Telegram.Substring(11, 8);
        Name = Telegram.Substring(19, 4);
        Data = Telegram.Substring(23);

        _logger.LogInformation($"AckFlag=[{AckFlag}]");
        _logger.LogInformation($"Counter=[{Counter}]");
        _logger.LogInformation($"Receiver=[{Receiver}]");
        _logger.LogInformation($"Sender=[{Sender}]");
        _logger.LogInformation($"Name=[{Name}]");
        _logger.LogInformation($"Data=[{Data}]");
        HexaDump();

        b = Bytes[0];
        if (b != DriverConstants.STX)
        {
            _logger.LogError($"Telegramm has wrong start byte: STX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]");
            return false;
        }

        b = Bytes[^1];
        if (b != DriverConstants.ETX)
        {
            _logger.LogError($"Telegramm has wrong end byte: ETX != [Hexa:0x{b.ToString("X2")} - Decimal:{b} - ASCII:{((char)b).ToString()}]");
            return false;
        }

        if (!Regex.IsMatch(AckFlag, @"^[0-1]$"))
        {
            _logger.LogError($"Telegram has invalid AckFlag=[{AckFlag}]. Expected values are: [0]=Acknowledge not required, [1]=Acknowledge required");
            return false;
        }

        if (!Regex.IsMatch(Counter, @"^[0-9]$"))
        {
            _logger.LogError($"Telegram has invalid Counter=[{Counter}]");
            return false;
        }

        if (Receiver != DriverConstants.GLOGWARE_IDENTIFIER)
        {
            _logger.LogError($"Telegram has an invalid Receiver: [{Receiver}] != [{DriverConstants.GLOGWARE_IDENTIFIER}]");
            return false;
        }

        if (!Regex.IsMatch(Name, DriverConstants.VALID_RECEIVING_NAMES))
        {
            _logger.LogError($"Telegram has an invalid Name=[{Name}] != [{DriverConstants.VALID_RECEIVING_NAMES}]");
            return false;
        }

        return true;
    }

    public string HexaDump()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine();
        string line = string.Empty;
        StringBuilder sbHexa = new StringBuilder();
        StringBuilder sbChar = new StringBuilder();

        for (int i = 0; i <= Bytes.Length; i++)
        {
            if (i % 10 == 0 || i == Bytes.Length)
            {
                if (line != string.Empty)
                {
                    sb.AppendLine($"{line}    {sbHexa.ToString().PadRight(30)}    {sbChar.ToString().PadRight(10)}");
                    sbHexa.Clear();
                    sbChar.Clear();
                }
                line = $"{i:0000}";
                if (i == Bytes.Length) break;
            }
            if (sbHexa.Length > 0) sbHexa.Append(" ");
            sbHexa.Append(Bytes[i].ToString("X2"));
            if (sbChar.Length > 0) sbChar.Append(" ");
            sbChar.Append((char)Bytes[i]).ToString();
        }
        _logger.LogInformation($"HexDump=[{sb.ToString()}]");
        return sb.ToString();
    }

}


/// <summary>
/// All valid telegram names exchanged between GLogWare and the bridges of the gantry PLC
/// </summary>
public enum PlcTelegramNames
{
    //////////////////////
    // PLC --> GLogWare //
    //////////////////////

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


    //////////////////////
    // GLogWare --> PLC //
    //////////////////////

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


    ///////////////////// 
    // Both directions //
    /////////////////////

    /// <summary>
    /// ACKNowledge telegram
    /// </summary>
    ACKN
}
