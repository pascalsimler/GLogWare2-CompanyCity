using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct ORDPStruct: ILegacyPlcStruct<ORDP, ORDSStruct>
{
    public static ORDSStruct FromData(string data)
    {
        throw new NotImplementedException();
    }

    public static ORDSStruct FromMessage(ORDP m)
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

    public ORDP ToMessage(string resourceNr)
    {
        throw new NotImplementedException();
    }
}