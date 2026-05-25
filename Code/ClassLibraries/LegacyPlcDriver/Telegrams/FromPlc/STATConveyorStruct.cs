using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct STATConveyorStruct : ILegacyPlcStruct<STATConveyor, STATConveyorStruct>
{
    public static STATConveyorStruct FromData(string data)
    {
        throw new NotImplementedException();
    }

    public static STATConveyorStruct FromMessage(STATConveyor m)
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

    public STATConveyor ToMessage(string resourceNr)
    {
        throw new NotImplementedException();
    }
}
