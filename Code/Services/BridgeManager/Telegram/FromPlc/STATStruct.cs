using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public struct STATStruct
{
    public string Parked;                   //   offset:000, Size: 001
    public string WorkingMode;              //   offset:001, Size: 001
    public string GripperOccupied;          //   offset:002, Size: 001
    public string ErrorFlag;                //   offset:003, Size: 001

    public static STATStruct FromData(string data)
    {
        STATStruct statStruct = new STATStruct();
        statStruct.Parked = data.Substring(0, 1);
        statStruct.WorkingMode = data.Substring(1, 1);
        statStruct.GripperOccupied = data.Substring(2, 1);
        statStruct.ErrorFlag = data.Substring(3, 1);

        return statStruct;
    }

    public (STATBridge, string) ToSTAT(string Bridge)
    {
        STATBridge stat = new STATBridge();

        string parked = string.Empty;
        string gripperOccupied = string.Empty;
        string errorFlag = string.Empty;

        if (Parked == "0")
        {
            stat.Parked = false;
            parked = "No";
        }
        else
        {
            stat.Parked = true;
            parked = "Yes";
        }


        stat.WorkingMode = (WorkingMode) switch {
            "1" => STATBridgeWorkingModes.AUTOMATIC,
            "3" => STATBridgeWorkingModes.MANUAL,
            "2" => STATBridgeWorkingModes.STOPPED,
            _ => STATBridgeWorkingModes.STOPPED
        };


        if (GripperOccupied == "0")
        {
            stat.GripperOccupied = false;
            gripperOccupied = "No";
        }
        else
        {
            stat.GripperOccupied = true;
            gripperOccupied = "Yes";
        }


        if (ErrorFlag == "0")
        {
            stat.ErrorFlag = false;
            errorFlag = "No";
        }
        else
        {
            stat.ErrorFlag = true;
            errorFlag = "Yes";
        }

        string logMsg =
            $"[ STATUS OF BRIDGE {Bridge} ]\r\n\r\n" +
            $"                      Parked: [{Parked}] - {parked}\r\n" +
            $"                Working Mode: [{WorkingMode}] - {stat.WorkingMode.ToString()}\r\n" +
            $"            Gripper Occupied: [{GripperOccupied}] - {gripperOccupied}\r\n" +
            $"                  Error Flag: [{ErrorFlag}] - {errorFlag}\r\n";


        return (stat, logMsg);
    }
}
