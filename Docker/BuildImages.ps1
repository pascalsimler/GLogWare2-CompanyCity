$esc = [char]27
$b = "$esc[1;37m"
$r = "$esc[0m"
$pathBackend = "..\Code\Services"
$pathFrontend = "..\Code\Frontend"
$pathRepository = "repository"
$pathSolution = ".."
$images = @(
    [tuple]::Create("demoservice", "$pathBackend\DemoService"), 
    [tuple]::Create("bridgemanager", "$pathBackend\BridgeManager"), 
    [tuple]::Create("bridgesimulator", "$pathBackend\BridgeSimulator"), 
    [tuple]::Create("simulatorwebapp", "$pathFrontend\SimulatorWebApp"), 
    [tuple]::Create("glogwarewebapp", "$pathFrontend\GLogWareWebApp\GLogWareWebApp")
)

cls
del "$pathRepository\*.tar"

$choice = "Z";
foreach ($image in $images) {
    if ($choice -notin @("A", "I")) {
        do {
            $choice =  Read-Host "Do you want to build the $b[$($image.Item1)]$r image? ($b[Y]$r=Yes, $b[N]$r=No, $b[A]$r=Yes to all, $b[I]$r=No to all)"
            $choice = $choice.ToUpper()
        } while ($choice -notin @("Y", "N", "A", "I"))
    }
    Write-Host "$b[$($image.Item1)]$r image ..."
    if ($choice -in @("Y", "A")) {
        docker build -f "$($image.Item2)\Dockerfile" --force-rm -t $($image.Item1) $pathSolution
        docker image save "$($image.Item1):latest" -o "$pathRepository\$($image.Item1).tar"
    }
}