using System;
using System.Collections.Generic;

namespace Gudel.GLogWare.Infrastructure.Views;

public partial class ViewStatisticBridgePerWeekConsolidated
{
    public int? Year { get; set; }

    public int? Week { get; set; }

    public int? CountStacksIn { get; set; }

    public int? CountStacksOut { get; set; }

    public int? CountStacksRelocated { get; set; }

    public int? CountStacksTotal { get; set; }

    public int? CountTiresIn { get; set; }

    public int? CountTiresOut { get; set; }

    public int? CountTiresRelocated { get; set; }

    public int? CountTiresTotal { get; set; }
}
