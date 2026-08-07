using Gudel.GLogWare.EFCore.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace GLogWareWebApp.Components.Pages.Visualization;

public partial class Visualization : IAsyncDisposable
{
    [Inject] private GLogWareDbContext _db { get; set; } = null!;

    private const double Ratio = 15.0; // ratio d'échelle pour l'affichage
    private double SvgWidth = 0;
    private double SvgHeight = 0;

    // Visual configuration (px)
    private double Radius = 475 / Ratio; // rayon d'un cercle
    private const double Padding = 12; // marge intérieure du rectangle
    private const string SelectedFill = "#ff7b7b"; // couleur quand sélectionné
    private const string UnselectedFill = "#bde0fe"; // couleur normale
    private const string SelectedStroke = "#b30000";
    private const string UnselectedStroke = "#0366d6";
    private const int FontSize = 40;

    private Random _rng = new();
    private List<Cell> _cells = new();
    private ElementReference _svgRef;
    private PeriodicTimer? _refreshTimer;
    private CancellationTokenSource? _refreshCts;

    private IEnumerable<Cell> SelectedCells => _cells.Where(c => c.Selected);

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _cells = new List<Cell>();

        LoadFromDatabase(true);

        SvgWidth = SvgWidth / Ratio + 2 * Radius;
        SvgHeight = SvgHeight / Ratio + 3 * Radius;

        _refreshCts = new CancellationTokenSource();
        _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _ = RefreshLoopAsync(_refreshCts.Token);
    }

    private async Task RefreshLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_refreshTimer != null && await _refreshTimer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(() =>
                {
                    LoadFromDatabase(false);
                    StateHasChanged();
                });
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void LoadFromDatabase(bool firstLoad)
    {
        var q = _db.Places.Where(x => x.Area == "GANTRY");

        foreach (var c in q)
        {
            if (firstLoad)
            {
                if (c.XPos > SvgWidth) SvgWidth = c.XPos;
                if (c.YPos > SvgHeight) SvgHeight = c.YPos;
            }

            var cell = _cells.Where(x => x.Row == int.Parse(c.YCell) && x.Col == int.Parse(c.XCell)).FirstOrDefault();
            if (cell == null)
            {
                cell = new Cell();
                cell.Row = int.Parse(c.YCell);
                cell.Col = int.Parse(c.XCell);
                _cells.Add(cell);
            }
            cell.Number = _rng.Next(1, 10); // 1..9 inclus
            cell.Selected = false;
            cell.Cx = Radius + c.XPos / Ratio;
            cell.Cy = 2 * Radius + c.YPos / Ratio;
            cell.Zob = $"#{_rng.Next(0x1000000):X6}";
        }
    }

    private void Toggle(Cell cell)
    {
        cell.Selected = !cell.Selected;
        if (cell.Selected)
        {
            cell.TempNumber = cell.Number;
        }
        StateHasChanged();
    }

    private void UpdateCellNumber(Cell cell)
    {
        if (cell.TempNumber >= 1 && cell.TempNumber <= 9)
        {
            cell.Number = cell.TempNumber;
            StateHasChanged();
        }
    }

    private void UpdateAllSelected()
    {
        foreach (var cell in SelectedCells)
        {
            if (cell.TempNumber >= 1 && cell.TempNumber <= 9)
            {
                cell.Number = cell.TempNumber;
            }
        }
        StateHasChanged();
    }

    private void ClearSelection()
    {
        foreach (var cell in _cells)
        {
            cell.Selected = false;
        }
        StateHasChanged();
    }

    private class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Number { get; set; }
        public bool Selected { get; set; }
        public double Cx { get; set; }
        public double Cy { get; set; }
        public int TempNumber { get; set; }
        public string? Zob { get; set; }
    }

    public ValueTask DisposeAsync()
    {
        _refreshCts?.Cancel();
        _refreshTimer?.Dispose();
        _refreshCts?.Dispose();
        return ValueTask.CompletedTask;
    }
}
