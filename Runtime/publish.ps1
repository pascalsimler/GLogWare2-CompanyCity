rm Services\BridgeManager -Recurse -Force
rm Services\BridgeSimulator -Recurse -Force
rm Services\ConveyorManager -Recurse -Force
rm Services\ConveyorSimulator -Recurse -Force
rm Services\JobManager -Recurse -Force
rm Services\Reserve -Recurse -Force
rm Services\Garbage -Recurse -Force
rm Services\Garbage -Recurse -Force
rm Frontend\SimulatorWebApp -Recurse -Force
rm Frontend\GLogWareWebApp -Recurse -Force

dotnet publish ..\Code\Services\BridgeManager -o Services\BridgeManager -r win-x64 -c Debug
dotnet publish ..\Code\Services\BridgeSimulator -o Services\BridgeSimulator -r win-x64 -c Debug
dotnet publish ..\Code\Services\ConveyorManager -o Services\ConveyorManager -r win-x64 -c Debug
dotnet publish ..\Code\Services\ConveyorSimulator -o Services\ConveyorSimulator -r win-x64 -c Debug
dotnet publish ..\Code\Services\JobManager -o Services\JobManager -r win-x64 -c Debug
dotnet publish ..\Code\Services\Reserve -o Services\Reserve -r win-x64 -c Debug
dotnet publish ..\Code\Services\Garbage -o Services\Garbage -r win-x64 -c Debug
dotnet publish ..\Code\Frontend\SimulatorWebApp -o Frontend\SimulatorWebApp -r win-x64 -c Debug
dotnet publish ..\Code\Frontend\GLogWareWebApp\GLogWareWebApp -o Frontend\GLogWareWebApp -r win-x64 -c Debug