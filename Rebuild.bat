dotnet clean YourSolution.slnx
dotnet nuget locals all --clear
dotnet build YourSolution.slnx --no-incremental
