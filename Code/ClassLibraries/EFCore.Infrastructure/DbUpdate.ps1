param(
	[Parameter(Mandatory = $true)]
	[ValidateSet("SqlServer", "Oracle", "Postgres", "MySql")]
    [string] $DatabaseProvider
)


switch ($DatabaseProvider) {
	"SqlServer" {
		$env:ConnectionStrings__Default="Server=sqlserver.glogware.com;Database=GLogWare_CompanyCity;User Id=sa;Password=*Gudel1954*;TrustServerCertificate=True"
	}
	"Postgres" {  
		$env:ConnectionStrings__Default="Host=postgres.glogware.com;Port=5432;Database=GLogWare_CompanyCity;Username=admin;Password=*Gudel1954*"
	}
	"Oracle" {  
		$env:ConnectionStrings__Default="User Id=CCI;Password=*Gudel1954*;Data Source=oracle.glogware.com:1521/GLogWare;"
	}
	"MySql" {
		$env:ConnectionStrings__Default="server=mysql.glogware.com;port=3306;database=GLogWare_CompanyCity;user=root;password=*Gudel1954*"
	}
	default {
		Write-Host "Invalid DataProvider !"
		exit
	}
}

$env:DatabaseProvider=$DatabaseProvider
Write-Host $env:ConnectionStrings__Default
dotnet ef database update