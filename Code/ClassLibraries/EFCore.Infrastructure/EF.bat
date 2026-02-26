$env:DatabaseProvider="SqlServer"  
$env:ConnectionStrings__Default="Server=sqlserver.glogware.com;Database=GLogWare_CompanyCity;User Id=sa;Password=*Gudel1954*;TrustServerCertificate=True"
dotnet ef migrations add Initial -o Migrations_SqlServer

$env:DatabaseProvider="Postgres"  
$env:ConnectionStrings__Default="Host=postgres.glogware.com;Port=5432;Database=GLogWare_CompanyCity;Username=admin;Password=*Gudel1954*"
dotnet ef migrations add Initial -o Migrations_Postgres

$env:DatabaseProvider="Oracle"  
$env:ConnectionStrings__Default="User Id=COC;Password=Oramgr001;Data Source=oracle.glogware.com:1521/FREEPDB1;"
dotnet ef migrations add Initial -o Migrations_Oracle

$env:DatabaseProvider="MySql"  
$env:ConnectionStrings__Default="server=mysql.glogware.com;port=3306;database=GLogWare_CompanyCity;user=root;password=*Gudel1954*"
dotnet ef migrations add Initial -o Migrations_MySql 


dotnet ef database update