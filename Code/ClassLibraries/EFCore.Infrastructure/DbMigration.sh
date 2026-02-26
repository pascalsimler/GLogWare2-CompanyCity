#!/bin/bash

# Usage function
usage() {
    echo "Usage: $0 <DatabaseProvider> <MigrationName>"
    echo "DatabaseProvider must be one of: SqlServer, Oracle, Postgres, MySql"
    exit 1
}

# Check if exactly 2 arguments are provided
if [ $# -ne 2 ]; then
    usage
fi

DatabaseProvider="$1"
MigrationName="$2"

# Validate DatabaseProvider
case "$DatabaseProvider" in
    "SqlServer"|"Oracle"|"Postgres"|"MySql")
        # valid
        ;;
    *)
        echo "Invalid DatabaseProvider!"
        usage
        ;;
esac

# Export DatabaseProvider as environment variable
export DatabaseProvider="$DatabaseProvider"

# Run dotnet ef migrations add
dotnet ef migrations add "$MigrationName" -o "Migrations_$DatabaseProvider"