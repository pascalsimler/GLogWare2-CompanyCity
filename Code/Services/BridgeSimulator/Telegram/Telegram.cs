using System.Text;

namespace Gudel.GLogWare.BridgeSimulator;

/// <summary>
/// GLogWare telegram representation
/// </summary>
public class Telegram
{
    public Telegram()
    {
        Bytes = new byte[DriverConstants.TELEGRAM_LENGTH];
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
    /// The telegram name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The datas of the telegram
    /// </summary>
    public string Data{ get; set; } = string.Empty;

    public void Build()
    {
        AsciiString = DriverConstants.TELEGRAM_TEMPLATE;
        AsciiString = AsciiString.Replace("[STX]", Convert.ToChar(DriverConstants.STX).ToString());
        AsciiString = AsciiString.Replace("[ETX]", Convert.ToChar(DriverConstants.ETX).ToString());
        AsciiString = AsciiString.Replace("[AckFlag]", AckFlag);
        AsciiString = AsciiString.Replace("[Counter]", Counter);
        AsciiString = AsciiString.Replace("[Receiver]", Receiver);
        AsciiString = AsciiString.Replace("[Sender]", Sender);
        AsciiString = AsciiString.Replace("[Name]", Name);
        AsciiString = AsciiString.Replace("[Data]", Data.PadRight(216, '.'));

        Array.Clear(Bytes, 0, Bytes.Length);
        byte[] tmpBuf = Encoding.ASCII.GetBytes(AsciiString);
        Array.Copy(tmpBuf, 0, Bytes, 0, tmpBuf.Length);
    }

    public void Parse()
    {
        AsciiString = Encoding.Default.GetString(Bytes, 0, Bytes.Length);
        AckFlag = AsciiString.Substring(1, 1);
        Counter = AsciiString.Substring(2, 1);
        Receiver = AsciiString.Substring(3, 8);
        Sender = AsciiString.Substring(11, 8);
        Name = AsciiString.Substring(19, 4);
        Data = AsciiString.Substring(23);
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

        return sb.ToString();
    }

}