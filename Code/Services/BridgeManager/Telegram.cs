using Gudel.GLogWare.Shared;
using System.Text;

namespace Gudel.GLogWare.BridgeManager;

/// <summary>
/// PLC telegram representation
/// </summary>
public class Telegram
{
    public Telegram()
    {
        Bytes = new byte[TelegramConstants.TELEGRAM_LENGTH];
    }

    /// <summary>
    /// byte array for buffering the received telegram so far
    /// </summary>
    public byte[] Bytes { get; set; }

    /// <summary>
    /// The complete content of the Plc telegram in a string
    /// </summary>
    public string AsciiString { get; set; } = string.Empty;

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
    /// The telegram identifier
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// The datas of the telegram
    /// </summary>
    public string Data{ get; set; } = string.Empty;


    public void Build()
    {
        AsciiString =
            Convert.ToChar(TelegramConstants.STX).ToString() +
            ((AckFlag.Length >= 1) ? AckFlag.Substring(0, 1) : AckFlag.PadRight(1)) +
            ((Counter.Length >= 1) ? Counter.Substring(0, 1) : Counter.PadRight(1)) +
            ((Receiver.Length >= 8) ? Receiver.Substring(0, 8) : Receiver.PadRight(8)) +
            ((Sender.Length >= 8) ? Sender.Substring(0, 8) : Sender.PadRight(8)) +
            ((Identifier.Length >= 4) ? Identifier.Substring(0, 4) : Identifier.PadRight(4)) +
            ((Data.Length >= 216) ? Data.Substring(0, 216) : Identifier.PadRight(216, '.')) +
            Convert.ToChar(TelegramConstants.ETX).ToString()
        ;

        Array.Clear(Bytes, 0, Bytes.Length);
        byte[] tmpBuf = Encoding.ASCII.GetBytes(AsciiString);
        Array.Copy(tmpBuf, 0, Bytes, 0, tmpBuf.Length);
    }

    public void Parse()
    {
        AsciiString = Encoding.ASCII.GetString(Bytes, 0, Bytes.Length);
        AckFlag = AsciiString.Substring(1, 1);
        Counter = AsciiString.Substring(2, 1);
        Receiver = AsciiString.Substring(3, 8);
        Sender = AsciiString.Substring(11, 8);
        Identifier = AsciiString.Substring(19, 4);
        Data = AsciiString.Substring(23, 216);
    }

    public string HexaDump()
    {
        byte b;
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
            b = Bytes[i];
            sbHexa.Append(b.ToString("X2"));
            char display = (b >= 0x20 && b < 0x7F) ? (char)b : '.';
            sbChar.Append(display);
        }

        return sb.ToString();
    }

    public void FromPlcMessage(PlcMessage pm)
    {
        Identifier = pm.Identifier.ToString();
        Sender = TelegramConstants.GLOGWARE_IDENTIFIER;
        Receiver = BridgeManager.OP;

        switch (pm.Identifier)
        {
            case PlcMessageIdentifiers.ORDS:
                ORDS r = GLogWareMessage.DeSerialize<ORDS>(pm.Data!.ToString()!)!;
                ORDSPosition p = r.PickPosition;
                ORDSPosition d = r.DropPosition;
                Data = 
                    ((r.Jobid.Length >= 16) ? r.Jobid.Substring(0, 16) : r.Jobid.PadRight(16)) +
                    $"{p.PositionType:0}" +
                    ((p.ConveyorPlace.Length > 8) ? p.ConveyorPlace.Substring(0, 8) : p.ConveyorPlace.PadRight(8)) +
                    $"{p.XCell:0000}" +
                    $"{p.YCell:0000}" +
                    $"{p.XPosition:000000}" +
                    $"{p.YPosition:000000}" +
                    $"{d.PositionType:0}" +
                    ((d.ConveyorPlace.Length > 8) ? d.ConveyorPlace.Substring(0, 8) : d.ConveyorPlace.PadRight(8)) +
                    $"{d.XCell:0000}" +
                    $"{d.YCell:0000}" +
                    $"{d.XPosition:000000}" +
                    $"{p.YPosition:000000}"
                ;
                break;
            default:
                Data = string.Empty;
                break;
        };
    }

    public PlcMessage ToPlcMessage()
    {
        return null!;
    }

}