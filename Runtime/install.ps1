sc.exe create "CCI-BridgeManager-OP7100BR" `
    binPath= "D:\GLogWare2\CompanyCity\Runtime\Services\BridgeManager\Gudel.GLogWare.Services.BridgeManager.exe"

New-ItemProperty `
    -Path "HKLM:\SYSTEM\CurrentControlSet\Services\CCI-BridgeManager-OP7100BR" `
    -Name "Environment" `
    -Value "OP=OP7100BR" `
    -PropertyType MultiString `
    -Force

sc.exe create "CCI-BridgeSimulator-OP7100BR" `
    binPath= "D:\GLogWare2\CompanyCity\Runtime\Services\BridgeSimulator\Gudel.GLogWare.Services.BridgeSimulator.exe"

New-ItemProperty `
    -Path "HKLM:\SYSTEM\CurrentControlSet\Services\CCI-BridgeSimulator-OP7100BR" `
    -Name "Environment" `
    -Value "OP=OP7100BR" `
    -PropertyType MultiString `
    -Force