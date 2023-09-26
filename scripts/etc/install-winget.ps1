# Install winget
$progressPreference = 'silentlyContinue'
Write-Information "Downloading WinGet and its dependencies..."
Invoke-WebRequest -Uri https://aka.ms/getwinget -OutFile Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle
Invoke-WebRequest -Uri https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx -OutFile Microsoft.VCLibs.x64.14.00.Desktop.appx
Invoke-WebRequest -Uri https://github.com/microsoft/microsoft-ui-xaml/releases/download/v2.7.3/Microsoft.UI.Xaml.2.7.x64.appx -OutFile Microsoft.UI.Xaml.2.7.x64.appx

Write-Host "Installing packages for winget"
Add-AppxPackage Microsoft.VCLibs.x64.14.00.Desktop.appx -Confirm:$false --accept-source-agreements --accept-package-agreements
Add-AppxPackage Microsoft.UI.Xaml.2.7.x64.appx -Confirm:$false --accept-source-agreements --accept-package-agreements
Add-AppxPackage Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle -Confirm:$false --accept-source-agreements --accept-package-agreements

# Remove the downloaded files after they've been added.
Remove-Item Microsoft.VCLibs.x64.14.00.Desktop.appx
Remove-Item Microsoft.UI.Xaml.2.7.x64.appx
Remove-Item Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle