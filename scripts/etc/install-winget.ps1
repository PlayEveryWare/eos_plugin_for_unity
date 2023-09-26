#. $PSScriptRoot/install-xaml.ps1
$progressPreference = 'silentlyContinue'
Write-Information "Downloading WinGet and its dependencies..."

Invoke-WebRequest -Uri https://aka.ms/getwinget -OutFile $PSScriptRoot/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle
Invoke-WebRequest -Uri https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx -OutFile $PSScriptRoot/Microsoft.VCLibs.x64.14.00.Desktop.appx
Invoke-WebRequest -Uri https://github.com/microsoft/microsoft-ui-xaml/releases/download/v2.7.3/Microsoft.UI.Xaml.2.7.x64.appx -OutFile $PSScript/Microsoft.UI.Xaml.2.7.x64.appx

Add-AppxPackage $PSScriptRoot/Microsoft.VCLibs.x64.14.00.Desktop.appx
Add-AppxPackage $PSScriptRoot/Microsoft.UI.Xaml.2.7.x64.appx
Add-AppxPackage $PSScriptRoot/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle
