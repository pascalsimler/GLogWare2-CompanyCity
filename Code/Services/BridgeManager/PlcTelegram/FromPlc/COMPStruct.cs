using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public struct COMPStruct
{     
    public string Jobid;            //Offset:000, Size: 016
    public string FeedbackCode;     //Offset:016, Size: 004

    public static COMPStruct FromData(string data)
    {
        COMPStruct c = new COMPStruct();
        c.Jobid = data.Substring(0, 16);
        c.FeedbackCode = data.Substring(16, 4);

        return c;
    }

    public (COMP, string) ToCOMP(string Bridge)
    {
        COMP c = new COMP();
        string logMsg = string.Empty;
  
        c.Jobid = Jobid.Trim();
        c.FeedbackCode = FeedbackCode;

        return (c, logMsg);
    }
}
