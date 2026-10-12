del Views\*.cs
dotnet ef dbcontext scaffold ^
    "Server=localhost;Database=GLogWare_CompanyCity;User Id=sa;Password=*Gudel1954*;TrustServerCertificate=True" ^
    Microsoft.EntityFrameworkCore.SqlServer ^
    --table vInventory ^
    --context GLogWareDbContext ^
    --context-dir Views ^
    --output-dir Views ^
    --namespace Gudel.GLogWare.Infrastructure.Views ^
    --context-namespace Gudel.GLogWare.Infrastructure ^
    --no-onconfiguring ^
    --force
