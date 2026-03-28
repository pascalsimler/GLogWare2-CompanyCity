using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public struct ORDSStruct
{
    public string Jobid;                         // Offset:000, Size: 016
    public string Article;                       // Offset:016, Size: 016
    public string Order;                         // Offset:032, Size: 016
    public string PickType;                      // Offset:048, Size: 001
    public string PickConvoyer;                  // Offset:049, Size: 004
    public string PickStoreXCell;                // Offset:053, Size: 004
    public string PickStoreYCell;                // Offset:057, Size: 004
    public string PickStoreXPosition;            // Offset:061, Size: 006
    public string PickStoreYPosition;            // Offset:067, Size: 006
    public string PickOffsetZ;                   // Offset:073, Size: 004           
    public string DropType;                      // Offset:077, Size: 001   
    public string DropConvoyer;                  // Offset:078, Size: 004
    public string DropStoreXCell;                // Offset:082, Size: 004
    public string DropStoreYCell;                // Offset:086, Size: 004
    public string DropStoreXPosition;            // Offset:092, Size: 006
    public string DropStoreYPosition;            // Offset:098, Size: 006
    public string DropOffsetZ;                   // Offset:104, Size: 004
    public string InnerDiameter;                 // Offset:108, Size: 004
    public string OuterDiameter;                 // Offset:112, Size: 004
    public string Width;                         // Offset:116, Size: 002
    public string TireCount;

    public static (ORDSStruct, string) FromORDS(string bridgeNr, ORDS o)
    {
        ORDSStruct os = new ORDSStruct();
        string logMsg = string.Empty;

        os.Jobid = (o.Jobid.Length >= 16) ? o.Jobid.Substring(0, 16) : o.Jobid.PadRight(16);
        os.Article = ((o.Article.Length >= 16) ? o.Article.Substring(0, 16) : o.Article.PadRight(16));
        os.Order = ((o.Order.Length >= 16) ? o.Order.Substring(0, 16) : o.Order.PadRight(16));

        ORDSPosition pp = o.PickPosition;
        os.PickType = $"{((int)pp.PositionType):0}";
        os.PickConvoyer = (pp.ConveyorPlace.Length > 4) ? pp.ConveyorPlace.Substring(0, 4) : pp.ConveyorPlace.PadRight(4);
        os.PickStoreXCell = $"{pp.XCell:0000}";
        os.PickStoreYCell = $"{pp.YCell:0000}";
        os.PickStoreXPosition = $"{pp.XPosition:000000}";
        os.PickStoreYPosition = $"{pp.YPosition:000000}";
        os.PickOffsetZ = $"{pp.ZOffset:0000}";

        ORDSPosition dp = o.DropPosition;
        os.DropType = $"{((int)dp.PositionType):0}";
        os.DropConvoyer = (dp.ConveyorPlace.Length > 4) ? dp.ConveyorPlace.Substring(0, 4) : dp.ConveyorPlace.PadRight(4);
        os.DropStoreXCell = $"{dp.XCell:0000}";
        os.DropStoreYCell = $"{dp.YCell:0000}";
        os.DropStoreXPosition = $"{dp.XPosition:000000}";
        os.DropStoreYPosition = $"{dp.YPosition:000000}";
        os.DropOffsetZ = $"{dp.ZOffset:0000}";

        os.InnerDiameter = $"{o.InnerDiameter:0000}";
        os.OuterDiameter = $"{o.OuterDiameter:0000}";
        os.Width = $"{o.Width:0000}";
        os.TireCount = $"{o.TireCount:00}";

        string movementType = string.Empty;
        if (pp.PositionType == ORDSPositionTypes.Conveyor && dp.PositionType == ORDSPositionTypes.Store)
            movementType = "INPUT        ";
        else if (pp.PositionType == ORDSPositionTypes.Store && dp.PositionType == ORDSPositionTypes.Conveyor)
            movementType = "OUTPUT       ";
        else if (pp.PositionType == ORDSPositionTypes.Store && dp.PositionType == ORDSPositionTypes.Store)
            movementType = "RELOCATION   ";
        else if (dp.PositionType == ORDSPositionTypes.Pallet)
            movementType = "PALLETIZATION";
        else
            movementType = "UNDEFINED    ";

        logMsg =
            $"[  BRIDGE {bridgeNr} - {movementType} ]\r\n" +
            $"\r\n" +
            $"                        Jobid: [{os.Jobid}]\r\n" +
            $"                      Article: [{os.Article}]\r\n" +
            $"                       Order : [{os.Order}]\r\n" +
            $"\r\n" +
            $"                Pick location: [{os.PickType}]-{pp.PositionType.ToString()}\r\n" +
            $"                Pick conveyor: [{os.PickConvoyer}]\r\n" +
            $"          Pick Store Cell X-Y: [{os.PickStoreXCell}]-[{os.PickStoreYCell}]\r\n" +
            $"      Pick Store Position X-Y: [{os.PickStoreXPosition}]-[{os.PickStoreYPosition}]\r\n" +
            $"                Pick Offset-Z: [{os.PickOffsetZ}]\r\n" +
            $"\r\n" +
            $"                Drop location: [{os.DropType}]-{dp.PositionType.ToString()}\r\n" +
            $"                Drop conveyor: [{os.DropConvoyer}]\r\n" +
            $"          Drop Store Cell X-Y: [{os.DropStoreXCell}]-[{os.DropStoreYCell}]\r\n" +
            $"      Drop Store Position X-Y: [{os.DropStoreXPosition}]-[{os.DropStoreYPosition}]\r\n" +
            $"                Drop Offset-Z: [{os.DropOffsetZ}]\r\n" +
            $"\r\n" +
            $"          Tire Inner-Diameter: [{os.InnerDiameter}]\r\n" +
            $"          Tire Outer-Diameter: [{os.OuterDiameter}]\r\n" +
            $"                   Tire Width: [{os.Width}]\r\n" +
            $"\r\n" +
            $"                  Amount tire: [{os.TireCount}]\r\n" +
            $"\r\n";

        return (os, logMsg);
    }

    public string ToData()
    {
        return
            Jobid +
            Article +
            Order +
            PickType +
            PickConvoyer +
            PickStoreXCell +
            PickStoreYCell +
            PickStoreXPosition +
            PickStoreYPosition +
            PickOffsetZ +
            DropType +
            DropConvoyer +
            DropStoreXCell +
            DropStoreYCell +
            DropStoreXPosition +
            DropStoreYPosition +
            DropOffsetZ +
            InnerDiameter +
            OuterDiameter +
            Width +
            TireCount;
    }
}
