using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeSimulator;

public struct COMPStruct
{
    public string Jobid;            //Offset:000, Size: 016
    public string FeedbackCode;     //Offset:016, Size: 004

    public static COMPStruct FromCOMP(COMP c)
    {
        COMPStruct cs = new COMPStruct();

        cs.Jobid = (c.Jobid.Length >= 16) ? c.Jobid.Substring(0, 16) : c.Jobid.PadRight(16);
        cs.FeedbackCode = (c.FeedbackCode.Length >= 4) ? c.FeedbackCode.Substring(0, 4) : c.FeedbackCode.PadRight(4);

        return cs;
    }

    public string ToData()
    {
        return
            Jobid +
            FeedbackCode;
    }
}
