using Gudel.GLogWare.Shared;

namespace Gudel.GLogWare.LegacyPlcDriver;

public struct ORDSStruct : ILegacyPlcStruct<ORDS, ORDSStruct>
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

    public static ORDSStruct FromMessage(ORDS m)
    {
        ORDSStruct s = new ORDSStruct();

        s.Jobid = (m.Jobid.Length >= 16) ? m.Jobid.Substring(0, 16) : m.Jobid.PadRight(16);
        s.Article = ((m.Article.Length >= 16) ? m.Article.Substring(0, 16) : m.Article.PadRight(16));
        s.Order = ((m.Order.Length >= 16) ? m.Order.Substring(0, 16) : m.Order.PadRight(16));

        ORDSPosition pp = m.PickPosition;
        s.PickType = $"{((int)pp.PositionType):0}";
        s.PickConvoyer = (pp.ConveyorPlace.Length > 4) ? pp.ConveyorPlace.Substring(0, 4) : pp.ConveyorPlace.PadRight(4);
        s.PickXCell = $"{pp.XCell:0000}";
        s.PickYCell = $"{pp.YCell:0000}";
        s.PickXPosition = $"{pp.XPosition:000000}";
        s.PickYPosition = $"{pp.YPosition:000000}";
        s.PickOffsetZ = $"{pp.ZOffset:0000}";
        
        ORDSPosition dp = m.DropPosition;
        s.DropType = $"{((int)dp.PositionType):0}";
        s.DropConvoyer = (dp.ConveyorPlace.Length > 4) ? dp.ConveyorPlace.Substring(0, 4) : dp.ConveyorPlace.PadRight(4);
        s.DropXCell = $"{dp.XCell:0000}";
        s.DropYCell = $"{dp.YCell:0000}";
        s.DropXPosition = $"{dp.XPosition:000000}";
        s.DropYPosition = $"{dp.YPosition:000000}";
        s.DropOffsetZ = $"{dp.ZOffset:0000}";

        s.InnerDiameter = $"{m.InnerDiameter:0000}";
        s.OuterDiameter = $"{m.OuterDiameter:0000}";
        s.Width = $"{m.Width:0000}";
        s.TireCount = $"{m.TireCount:00}";

        return s;
    }

    public ORDS ToMessage(string resourceNr)
    {
        ORDS o = new ORDS();

        o.Jobid = Jobid.Trim();
        o.Article = Article.Trim();
        o.Order = Order.Trim();

        ORDSPosition pp = o.PickPosition; 
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

        ORDSPosition dp = o.DropPosition;
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

        o.InnerDiameter = int.TryParse(InnerDiameter, out int innerDiameter) ? innerDiameter : 0;
        o.OuterDiameter = int.TryParse(OuterDiameter, out int outerDiameter) ? outerDiameter : 0;
        o.Width = int.TryParse(Width, out int width) ? width : 0;

        o.TireCount = int.TryParse(TireCount, out int tireCount) ? tireCount : 1;    

        return o;
    }

    public string ToLogMessage(string resourceNr)
    {
        ORDS o = ToMessage(resourceNr); 
        
        string movementType = string.Empty;
        if (
            o.PickPosition.PositionType == ORDSPositionTypes.Conveyor && 
            o.DropPosition.PositionType == ORDSPositionTypes.Store
        )
            movementType = "INPUT        ";
        else if (
            o.PickPosition.PositionType == ORDSPositionTypes.Store && 
            o.DropPosition.PositionType == ORDSPositionTypes.Conveyor
        )
            movementType = "OUTPUT       ";
        else if (
            o.PickPosition.PositionType == ORDSPositionTypes.Store && 
            o.DropPosition.PositionType == ORDSPositionTypes.Store
        )
            movementType = "RELOCATION   ";
        else if (
            o.DropPosition.PositionType == ORDSPositionTypes.Pallet
        )
            movementType = "PALLETIZATION";
        else
            movementType = "UNDEFINED    ";

        string logMsg =
           $"[  BRIDGE {resourceNr} - {movementType} ]\r\n" +
           $"\r\n" +
           $"                        Jobid: [{Jobid}]\r\n" +
           $"                      Article: [{Article}]\r\n" +
           $"                       Order : [{Order}]\r\n" +
           $"\r\n" +
           $"                Pick location: [{PickType}]-{o.PickPosition.PositionType.ToString()}\r\n" +
           $"                Pick conveyor: [{PickConvoyer}]\r\n" +
           $"                Pick Cell X-Y: [{PickXCell}]-[{PickYCell}]\r\n" +
           $"            Pick Position X-Y: [{PickXPosition}]-[{PickYPosition}]\r\n" +
           $"                Pick Offset-Z: [{PickOffsetZ}]\r\n" +
           $"\r\n" +
           $"                Drop location: [{DropType}]-{o.DropPosition.PositionType.ToString()}\r\n" +
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
