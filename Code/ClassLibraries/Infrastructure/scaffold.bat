del Views\*.cs
dotnet ef dbcontext scaffold ^
    "Server=localhost;Database=GLogWare_CompanyCity;User Id=sa;Password=*Gudel1954*;TrustServerCertificate=True" ^
    Microsoft.EntityFrameworkCore.SqlServer ^
    --table ViewInventory ^
    --table ViewStatisticFillLevel ^
    --table ViewStatisticBridgePerHour ^
    --table ViewStatisticBridgePerHourConsolidated ^
    --table ViewStatisticBridgePerDay ^
    --table ViewStatisticBridgePerDayConsolidated ^
    --context GLogWareDbContext ^
    --context-dir Views ^
    --output-dir Views ^
    --namespace Gudel.GLogWare.Infrastructure.Views ^
    --context-namespace Gudel.GLogWare.Infrastructure ^
    --no-onconfiguring ^
    --force
