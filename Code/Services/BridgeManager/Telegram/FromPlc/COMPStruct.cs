using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public struct COMPStruct
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

    public static COMPStruct FromCOMP(COMP c)
    {
        COMPStruct cs = new COMPStruct();

        cs.Jobid = (c.Jobid.Length >= 16) ? c.Jobid.Substring(0, 16) : c.Jobid.PadRight(16);
        cs.FeedbackCode = (c.FeedbackCode.Length >= 4) ? c.FeedbackCode.Substring(0, 4) : c.FeedbackCode.PadRight(4);

        return cs;
    }

    public (COMP, string) ToCOMP(string Bridge)
    {
        COMP comp = new COMP();
        comp.Jobid = Jobid.Trim();
        comp.FeedbackCode = FeedbackCode;

        string logMsg =
             $"[ ORDER COMPLETED {Bridge} ]\r\n" +
             $"\r\n" +
             $"           JobId: [{Jobid}]\r\n" +
             $"   Feedback Code: [{FeedbackCode}]\r\n" +
             $"\r\n";

        return (comp, logMsg);
    }
}
