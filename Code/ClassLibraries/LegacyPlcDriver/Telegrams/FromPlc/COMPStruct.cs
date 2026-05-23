using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct COMPStruct: ILegacyPlcStruct<COMP, COMPStruct>
{     
    public string Jobid;            //Offset:000, Size: 016
    public string FeedbackCode;     //Offset:016, Size: 004

    public static COMPStruct FromData(string data)
    {
        COMPStruct compStruct = new COMPStruct();
        compStruct.Jobid = data.Substring(0, 16);
        compStruct.FeedbackCode = data.Substring(16, 4);

        return compStruct;
    }

    public string ToData()
    {
        return
            Jobid +
            FeedbackCode;
    }

    public static COMPStruct FromMessage(COMP c)
    {
        COMPStruct cs = new COMPStruct();

        cs.Jobid = (c.Jobid.Length >= 16) ? c.Jobid.Substring(0, 16) : c.Jobid.PadRight(16);
        cs.FeedbackCode = (c.FeedbackCode.Length >= 4) ? c.FeedbackCode.Substring(0, 4) : c.FeedbackCode.PadRight(4);

        return cs;
    }

    public COMP ToMessage(string resourceNr)
    {
        COMP comp = new COMP();
        comp.Jobid = Jobid.Trim();
        comp.FeedbackCode = FeedbackCode;

        return comp;
    }

    public string ToLogMessage(string elementNr)
    {
        COMP comp = new COMP();
        comp.Jobid = Jobid.Trim();
        comp.FeedbackCode = FeedbackCode;

        string logMsg =
             $"[ ORDER COMPLETED {elementNr} ]\r\n" +
             $"\r\n" +
             $"           JobId: [{Jobid}]\r\n" +
             $"   Feedback Code: [{FeedbackCode}]\r\n" +
             $"\r\n";

        return logMsg;
    }
}
