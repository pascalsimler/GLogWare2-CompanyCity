namespace Gudel.GLogWare.Shared;

public interface ILegacyPlcStruct<TPlcMessage, TPlcStruct>
{
    static abstract TPlcStruct FromData(string data);
    string ToData();
    static abstract TPlcStruct FromMessage(TPlcMessage m);
    TPlcMessage ToMessage(string resourceNr);
    string ToLogMessage(string resourceNr);
}
