using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct ARIVStruct : ILegacyPlcStruct<COMP, COMPStruct>
{
    public static COMPStruct FromData(string data)
    {
        throw new NotImplementedException();
    }

    public static COMPStruct FromMessage(COMP m)
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

    public COMP ToMessage(string resourceNr)
    {
        throw new NotImplementedException();
    }
}
