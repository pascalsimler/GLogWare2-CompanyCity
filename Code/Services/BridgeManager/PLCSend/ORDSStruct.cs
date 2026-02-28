using Gudel.GLogWare.Shared;
using System.Runtime.InteropServices;

namespace Gudel.GLogWare.BridgeManager;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ORDSStruct
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string JobId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
    public string JobType;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
    public string PositionType;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
    public string ConveyorNumber;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
    public string StoreCellX;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
    public string StoreCellY;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
    public string OffsetZ1;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
    public string OffsetZ2;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
    public string OffsetX;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
    public string OffsetY;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
    public string Orientation;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
    public string PreCentering;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
    public string AmountSkusOnPlace;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
    public string AmountSkusInGripper;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
    public string CrateLength;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
    public string CrateWidth;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
    public string OffsetZ3;

    public static ORDSStruct FromORDS(ORDS r) => new ORDSStruct
    {
        JobId = string.Empty,
        JobType = string.Empty,
        PositionType = string.Empty,
        ConveyorNumber = string.Empty,
        StoreCellX = string.Empty,
        StoreCellY = string.Empty,
        OffsetZ1 = string.Empty,
        OffsetZ2 = string.Empty,
        OffsetX = string.Empty,
        OffsetY = string.Empty,
        Orientation = string.Empty,
        PreCentering = string.Empty,
        AmountSkusOnPlace = string.Empty,
        AmountSkusInGripper = string.Empty,
        CrateLength = string.Empty,
        CrateWidth = string.Empty,
        OffsetZ3 = string.Empty
    };
}
