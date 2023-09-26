. $PSScriptRoot/install-xaml.ps1
$progressPreference = 'silentlyContinue'
Write-Information "Downloading WinGet and its dependencies..."
Invoke-WebRequest -Uri https://aka.ms/getwinget -OutFile Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle
Invoke-WebRequest -Uri https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx -OutFile Microsoft.VCLibs.x64.14.00.Desktop.appx
Invoke-WebRequest -Uri https://github.com/microsoft/microsoft-ui-xaml/releases/download/v2.7.3/Microsoft.UI.Xaml.2.7.x64.appx -OutFile Microsoft.UI.Xaml.2.7.x64.appx
Add-AppxPackage Microsoft.VCLibs.x64.14.00.Desktop.appx
Add-AppxPackage Microsoft.UI.Xaml.2.7.x64.appx
Add-AppxPackage Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle

#Write-Host "Installing latest version of winget."

# Install the NuGet package provider
#Write-Host " - installing NuGet package provider."
#Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force -Scope CurrentUser | Out-Null

# Trust the PSGallery repository.
#Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted

# Install the winget tools stuff.
#Write-Host " - installing module wingettools"

#Install-Module -Name WingetTools -Scope CurrentUser

#Write-Host " - running `"Install-Winget`"."
#Install-WinGet

# Upgrade winget to the newest version.
#Write-Host " - upgrading winget to the latest version."
#winget upgrade --all --silent --accept-package-agreements --accept-source-agreements --force