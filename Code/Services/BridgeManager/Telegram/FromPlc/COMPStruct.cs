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

    public (COMP, string) ToCOMP(string Bridge)
    {
        COMP comp = new COMP();
        comp.Bridge = Bridge;
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
