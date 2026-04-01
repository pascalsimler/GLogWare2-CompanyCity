using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.BridgeManager;

public struct ORDSStruct
{
    public string Jobid;
    public string Article;
    public string Order;
    public string PickType;
    public string PickConvoyer;
    public string PickXCell;
    public string PickYCell;
    public string PickXPosition;
    public string PickYPosition;
    public string PickOffsetZ;         
    public string DropType;
    public string DropConvoyer;
    public string DropXCell;
    public string DropYCell;
    public string DropXPosition;
    public string DropYPosition;
    public string DropOffsetZ;
    public string InnerDiameter;
    public string OuterDiameter;
    public string Width;
    public string TireCount;

    public static ORDSStruct FromData(string data)
    {
        ORDSStruct ordsStruct = new ORDSStruct();

        ordsStruct.Jobid = data.Substring(0,16);
        ordsStruct.Article = data.Substring(16, 16);
        ordsStruct.Order = data.Substring(32, 16);
        ordsStruct.PickType = data.Substring(48, 1);
        ordsStruct.PickConvoyer = data.Substring(49, 4);
        ordsStruct.PickXCell = data.Substring(53, 4);
        ordsStruct.PickYCell = data.Substring(57, 4);
        ordsStruct.PickXPosition = data.Substring(61, 6);
        ordsStruct.PickYPosition = data.Substring(67, 6);
        ordsStruct.PickOffsetZ = data.Substring(73, 4);
        ordsStruct.DropType = data.Substring(77, 1);
        ordsStruct.DropConvoyer = data.Substring(78, 4);          
        ordsStruct.DropXCell = data.Substring(82, 4);  
        ordsStruct.DropYCell = data.Substring(86, 4); 
        ordsStruct.DropXPosition = data.Substring(90, 6);
        ordsStruct.DropYPosition = data.Substring(96, 6);      
        ordsStruct.DropOffsetZ = data.Substring(102, 4);    
        ordsStruct.InnerDiameter = data.Substring(106, 4);
        ordsStruct.OuterDiameter = data.Substring(110, 4);
        ordsStruct.Width = data.Substring(114, 4);
        ordsStruct.TireCount = data.Substring(118, 2);

        return ordsStruct;
    }

    public string ToData()
    {
        return
            Jobid +
            Article +
            Order +
            PickType +
            PickConvoyer +
            PickXCell +
            PickYCell +
            PickXPosition +
            PickYPosition +
            PickOffsetZ +
            DropType +
            DropConvoyer +
            DropXCell +
            DropYCell +
            DropXPosition +
            DropYPosition +
            DropOffsetZ +
            InnerDiameter +
            OuterDiameter +
            Width +
            TireCount;
    }

    public static ORDSStruct FromORDS(ORDS o)
    {
        ORDSStruct os = new ORDSStruct();

        os.Jobid = (o.Jobid.Length >= 16) ? o.Jobid.Substring(0, 16) : o.Jobid.PadRight(16);
        os.Article = ((o.Article.Length >= 16) ? o.Article.Substring(0, 16) : o.Article.PadRight(16));
        os.Order = ((o.Order.Length >= 16) ? o.Order.Substring(0, 16) : o.Order.PadRight(16));

        ORDSPosition pp = o.PickPosition;
        os.PickType = $"{((int)pp.PositionType):0}";
        os.PickConvoyer = (pp.ConveyorPlace.Length > 4) ? pp.ConveyorPlace.Substring(0, 4) : pp.ConveyorPlace.PadRight(4);
        os.PickXCell = $"{pp.XCell:0000}";
        os.PickYCell = $"{pp.YCell:0000}";
        os.PickXPosition = $"{pp.XPosition:000000}";
        os.PickYPosition = $"{pp.YPosition:000000}";
        os.PickOffsetZ = $"{pp.ZOffset:0000}";

        ORDSPosition dp = o.DropPosition;
        os.DropType = $"{((int)dp.PositionType):0}";
        os.DropConvoyer = (dp.ConveyorPlace.Length > 4) ? dp.ConveyorPlace.Substring(0, 4) : dp.ConveyorPlace.PadRight(4);
        os.DropXCell = $"{dp.XCell:0000}";
        os.DropYCell = $"{dp.YCell:0000}";
        os.DropXPosition = $"{dp.XPosition:000000}";
        os.DropYPosition = $"{dp.YPosition:000000}";
        os.DropOffsetZ = $"{dp.ZOffset:0000}";

        os.InnerDiameter = $"{o.InnerDiameter:0000}";
        os.OuterDiameter = $"{o.OuterDiameter:0000}";
        os.Width = $"{o.Width:0000}";
        os.TireCount = $"{o.TireCount:00}";

        return os;
    }

    public ORDS ToORDS()
    {
        ORDS ords = new ORDS();

        ords.Jobid = Jobid.Trim();
        ords.Article = Article.Trim();
        ords.Order = Order.Trim();

        ORDSPosition pp = ords.PickPosition; 
        if (int.TryParse(PickType, out int intPickType) &&
            Enum.IsDefined(typeof(ORDSPositionTypes), intPickType))
        {
            pp.PositionType = (ORDSPositionTypes)intPickType;
        }
        pp.ConveyorPlace = PickConvoyer.Trim();
        pp.XCell = int.TryParse(PickXCell, out int pickXCell) ? pickXCell : 0;
        pp.YCell = int.TryParse(PickYCell, out int pickYCell) ? pickYCell : 0;
        pp.XPosition = int.TryParse(PickXPosition, out int pickXPosition) ? pickXPosition : 0;
        pp.YPosition = int.TryParse(PickYPosition, out int pickYPosition) ? pickYPosition : 0;
        pp.ZOffset = int.TryParse(PickOffsetZ, out int pickZOffset) ? pickZOffset : 0;

        ORDSPosition dp = ords.DropPosition;
        if (int.TryParse(DropType, out int intDropType) &&
            Enum.IsDefined(typeof(ORDSPositionTypes), intDropType))
        {
            dp.PositionType = (ORDSPositionTypes)intDropType;
        }
        dp.XCell = int.TryParse(DropXCell, out int dropXCell) ? dropXCell : 0;
        dp.YCell = int.TryParse(DropYCell, out int dropYCell) ? dropYCell : 0;
        dp.XPosition = int.TryParse(DropXPosition, out int dropXPosition) ? dropXPosition : 0;
        dp.YPosition = int.TryParse(DropYPosition, out int dropYPosition) ? dropYPosition : 0;
        dp.ZOffset = int.TryParse(DropOffsetZ, out int dropZOffset) ? dropZOffset : 0;

        ords.InnerDiameter = int.TryParse(InnerDiameter, out int innerDiameter) ? innerDiameter : 0;
        ords.OuterDiameter = int.TryParse(OuterDiameter, out int outerDiameter) ? outerDiameter : 0;
        ords.Width = int.TryParse(Width, out int width) ? width : 0;

        ords.TireCount = int.TryParse(TireCount, out int tireCount) ? tireCount : 1;    

        return ords;
    }

    public string ToLogMessage(string bridgeNr)
    {
        ORDS ords = ToORDS(); 
        
        string movementType = string.Empty;
        if (
            ords.PickPosition.PositionType == ORDSPositionTypes.Conveyor && 
            ords.DropPosition.PositionType == ORDSPositionTypes.Store
        )
            movementType = "INPUT        ";
        else if (
            ords.PickPosition.PositionType == ORDSPositionTypes.Store && 
            ords.DropPosition.PositionType == ORDSPositionTypes.Conveyor
        )
            movementType = "OUTPUT       ";
        else if (
            ords.PickPosition.PositionType == ORDSPositionTypes.Store && 
            ords.DropPosition.PositionType == ORDSPositionTypes.Store
        )
            movementType = "RELOCATION   ";
        else if (
            ords.DropPosition.PositionType == ORDSPositionTypes.Pallet
        )
            movementType = "PALLETIZATION";
        else
            movementType = "UNDEFINED    ";

        string logMsg =
           $"[  BRIDGE {bridgeNr} - {movementType} ]\r\n" +
           $"\r\n" +
           $"                        Jobid: [{Jobid}]\r\n" +
           $"                      Article: [{Article}]\r\n" +
           $"                       Order : [{Order}]\r\n" +
           $"\r\n" +
           $"                Pick location: [{PickType}]-{ords.PickPosition.PositionType.ToString()}\r\n" +
           $"                Pick conveyor: [{PickConvoyer}]\r\n" +
           $"                Pick Cell X-Y: [{PickXCell}]-[{PickYCell}]\r\n" +
           $"            Pick Position X-Y: [{PickXPosition}]-[{PickYPosition}]\r\n" +
           $"                Pick Offset-Z: [{PickOffsetZ}]\r\n" +
           $"\r\n" +
           $"                Drop location: [{DropType}]-{ords.DropPosition.PositionType.ToString()}\r\n" +
           $"                Drop conveyor: [{DropConvoyer}]\r\n" +
           $"                Drop Cell X-Y: [{DropXCell}]-[{DropYCell}]\r\n" +
           $"            Drop Position X-Y: [{DropXPosition}]-[{DropYPosition}]\r\n" +
           $"                Drop Offset-Z: [{DropOffsetZ}]\r\n" +
           $"\r\n" +
           $"          Tire Inner-Diameter: [{InnerDiameter}]\r\n" +
           $"          Tire Outer-Diameter: [{OuterDiameter}]\r\n" +
           $"                   Tire Width: [{Width}]\r\n" +
           $"\r\n" +
           $"                  Amount tire: [{TireCount}]\r\n" +
           $"\r\n";

        return logMsg;
    }
}
