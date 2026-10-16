using System;
using System.Collections.Generic;

namespace Gudel.GLogWare.Infrastructure.Views;

public partial class ViewStatisticFillLevel
{
    public string Area { get; set; } = null!;

    public string? Day { get; set; }

    public int Hour { get; set; }

    public int CountTotal { get; set; }

    public int CountAvailable { get; set; }

    public int? PercentAvailable { get; set; }

    public int CountBlocked { get; set; }

    public int? PercentBlocked { get; set; }

    public int CountFree { get; set; }

    public int? PercentFree { get; set; }

    public int? CountOccupied { get; set; }

    public int? PercentOccupied { get; set; }
}
