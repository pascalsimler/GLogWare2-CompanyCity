using System;
using System.Collections.Generic;

namespace Gudel.GLogWare.Infrastructure.Views;

public partial class ViewStatisticBridgePerDayConsolidated
{
    public string BridgeNr { get; set; } = null!;

    public DateOnly Day { get; set; }

    public string Hour { get; set; } = null!;

    public int? CountStacksIn { get; set; }

    public int? CountStacksOut { get; set; }

    public int? CountStacksRelocated { get; set; }

    public int? CountStacksTotal { get; set; }

    public int? CountTiresIn { get; set; }

    public int? CountTiresOut { get; set; }

    public int? CountTiresRelocated { get; set; }

    public int? CountTiresTotal { get; set; }
}
