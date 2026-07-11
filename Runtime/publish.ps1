dotnet publish ..\Code\Services\BridgeManager -o BridgeManager -r win-x64 -c Debug -p:PublishSingleFile=true
dotnet publish ..\Code\Services\BridgeSimulator -o BridgeSimulator -r win-x64 -c Debug
dotnet publish ..\Code\Services\ConveyorManager -o ConveyorManager -r win-x64 -c Debug
dotnet publish ..\Code\Services\ConveyorSimulator -o ConveyorSimulator -r win-x64 -c Debug
dotnet publish ..\Code\Services\JobManager -o JobManager -r win-x64 -c Debug
dotnet publish ..\Code\Services\Reserve -o Reserve -r win-x64 -c Debug
dotnet publish ..\Code\Services\Garbage -o Garbage -r win-x64 -c Debug