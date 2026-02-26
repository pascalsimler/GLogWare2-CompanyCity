param(
	[Parameter(Mandatory = $true)]
	[ValidateSet("SqlServer", "Oracle", "Postgres", "MySql")]
    [string] $DatabaseProvider,
    
	[Parameter(Mandatory = $true)]
	[string] $MigrationName
)

$env:DatabaseProvider=$DatabaseProvider
dotnet ef migrations add $MigrationName -o "Migrations_$DatabaseProvider"