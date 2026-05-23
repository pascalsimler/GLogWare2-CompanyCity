#!/bin/bash

# Usage function
usage() {
    echo "Usage: $0 <DatabaseProvider>"
    echo "DatabaseProvider must be one of: SqlServer, Oracle, Postgres, MySql"
    exit 1
}

# Check if exactly 1 argument is provided
if [ $# -ne 1 ]; then
    usage
fi

DatabaseProvider="$1"

# Convert input to match case (optional)
case "$DatabaseProvider" in
    "SqlServer")
        export ConnectionStrings__Default="Server=sqlserver.glogware.com;Database=GLogWare_CompanyCity;User Id=sa;Password=*Gudel1954*;TrustServerCertificate=True"
        ;;
    "Postgres")
        export ConnectionStrings__Default="Host=postgres.glogware.com;Port=5432;Database=GLogWare_CompanyCity;Username=admin;Password=*Gudel1954*"
        ;;
    "Oracle")
        export ConnectionStrings__Default="User Id=COC;Password=Oramgr001;Data Source=oracle.glogware.com:1521/FREEPDB1;"
        ;;
    "MySql")
        export ConnectionStrings__Default="server=mysql.glogware.com;port=3306;database=GLogWare_CompanyCity;user=root;password=*Gudel1954*"
        ;;
    *)
        echo "Invalid DatabaseProvider!"
        usage
        ;;
esac

# Export DatabaseProvider variable
export DatabaseProvider="$DatabaseProvider"

# Print the connection string
echo "$ConnectionStrings__Default"

# Run dotnet ef database update
dotnet ef database update