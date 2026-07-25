using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct ARIVStruct : ILegacyPlcStruct<ARIV, ARIVStruct>
{
    public static ARIVStruct FromData(string data)
    {
        throw new NotImplementedException();
    }

    public static ARIVStruct FromMessage(ARIV m)
    {
        throw new NotImplementedException();
    }

    public string ToData()
    {
        throw new NotImplementedException();
    }

    public string ToLogMessage(string resourceNr)
    {
        throw new NotImplementedException();
    }

    public ARIV ToMessage(string resourceNr)
    {
        throw new NotImplementedException();
    }
}
