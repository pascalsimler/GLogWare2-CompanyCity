using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct TARGStruct : ILegacyPlcStruct<TARG, TARGStruct>
{
    public static TARGStruct FromData(string data)
    {
        throw new NotImplementedException();
    }

    public static TARGStruct FromMessage(TARG m)
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

    public TARG ToMessage(string resourceNr)
    {
        throw new NotImplementedException();
    }
}