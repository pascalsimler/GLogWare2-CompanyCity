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
        STATStruct s = new STATStruct();
        s.Parked = data.Substring(0, 1);
        s.WorkingMode = data.Substring(1, 1);
        s.GripperOccupied = data.Substring(2, 1);
        s.ErrorFlag = data.Substring(3, 1);

        return s;
    }

    public (STATBridge, string) ToSTAT(string Bridge)
    {
        STATBridge s = new STATBridge();
        string logMsg = string.Empty;
        string parked = string.Empty;
        string gripperOccupied = string.Empty;
        string errorFlag = string.Empty;

        if (Parked == "0")
        {
            s.Parked = false;
            parked = "No";
        }
        else
        {
            s.Parked = true;
            parked = "Yes";
        }
          

        switch (WorkingMode)
        {
            case "1":
                s.WorkingMode = STATBridgeWorkingModes.AUTOMATIC;
                break;
            case "3":
                s.WorkingMode = STATBridgeWorkingModes.MANUAL;
                break;
            case "2":
            default:
                s.WorkingMode = STATBridgeWorkingModes.STOPPED;
                break;
        }


        if (GripperOccupied == "0")
        {
            s.GripperOccupied = false;
            gripperOccupied = "No";
        }
        else
        {
            s.GripperOccupied = true;
            gripperOccupied = "Yes";
        }


        if (ErrorFlag == "0")
        {
            s.ErrorFlag = false;
            errorFlag = "No";
        }
        else
        {
            s.ErrorFlag = true;
            errorFlag = "Yes";
        }

        logMsg =
            $"[ STATUS OF BRIDGE {Bridge} ]\r\n\r\n" +
            $"                      Parked: [{Parked}] - {parked}\r\n" +
            $"                Working Mode: [{WorkingMode}] - {s.WorkingMode.ToString()}\r\n" +
            $"               GripperStatus: [{GripperOccupied}] - {gripperOccupied}\r\n" +
            $"                  Error Flag: [{ErrorFlag}] - {errorFlag}\r\n";


        return (s, logMsg);
    }
}
