using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeSimulator;

public struct STATStruct
{
    public string Parked;                   //   offset:000, Size: 001
    public string WorkingMode;              //   offset:001, Size: 001
    public string GripperOccupied;          //   offset:002, Size: 001
    public string ErrorFlag;                //   offset:003, Size: 001

    public static STATStruct FromSTAT(STATBridge s)
    {
        STATStruct ss = new STATStruct();
        
        ss.Parked = s.Parked ? "1" : "0";
        ss.WorkingMode = s.WorkingMode switch
        {
            STATBridgeWorkingModes.AUTOMATIC => "1",
            STATBridgeWorkingModes.MANUAL => "3",
            STATBridgeWorkingModes.STOPPED => "2",
            _ => "0",
        };
        ss.GripperOccupied = s.GripperOccupied ? "1" : "0";
        ss.ErrorFlag = s.ErrorFlag ? "1" : "0";

        return ss;
    }

    public string ToData()
    {
        return
            Parked +
            WorkingMode +
            GripperOccupied +
            ErrorFlag;
    }
}
