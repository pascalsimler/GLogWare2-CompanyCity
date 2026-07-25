using Gudel.GLogWare.Messages;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct STATBridgeStruct: ILegacyPlcStruct<STATBridge, STATBridgeStruct>
{
    public string Parked;
    public string WorkingMode;
    public string GripperOccupied;
    public string ErrorFlag;

    public static STATBridgeStruct FromData(string data)
    {
        STATBridgeStruct statStruct = new STATBridgeStruct();
        statStruct.Parked = data.Substring(0, 1);
        statStruct.WorkingMode = data.Substring(1, 1);
        statStruct.GripperOccupied = data.Substring(2, 1);
        statStruct.ErrorFlag = data.Substring(3, 1);

        return statStruct;
    }

    public string ToData()
    {
        return
            Parked +
            WorkingMode +
            GripperOccupied +
            ErrorFlag;
    }

    public static STATBridgeStruct FromMessage(STATBridge s)
    {
        STATBridgeStruct ss = new STATBridgeStruct();

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

    public STATBridge ToMessage(string resourceNr)
    {
        STATBridge stat = new STATBridge();

        stat.Parked = (Parked != "0");
        stat.GripperOccupied = (GripperOccupied != "0");
        stat.ErrorFlag = (ErrorFlag != "0");

        stat.WorkingMode = (WorkingMode) switch {
            "1" => STATBridgeWorkingModes.AUTOMATIC,
            "3" => STATBridgeWorkingModes.MANUAL,
            "2" => STATBridgeWorkingModes.STOPPED,
            _ => STATBridgeWorkingModes.STOPPED
        };

        return stat;
    }

    public string ToLogMessage(string resourceNr)
    {
        STATBridge stat = ToMessage(resourceNr);

        string parked = (stat.Parked) ? "Yes" : "No";
        string gripperOccupied = (stat.GripperOccupied) ? "Yes" : "No";
        string errorFlag = (stat.ErrorFlag) ? "Yes" : "No";

        string logMsg =
            $"[ STATUS OF BRIDGE {resourceNr} ]\r\n\r\n" +
            $"                      Parked: [{Parked}] - {parked}\r\n" +
            $"                Working Mode: [{WorkingMode}] - {stat.WorkingMode.ToString()}\r\n" +
            $"            Gripper Occupied: [{GripperOccupied}] - {gripperOccupied}\r\n" +
            $"                  Error Flag: [{ErrorFlag}] - {errorFlag}\r\n";

        return logMsg;
    }
}
