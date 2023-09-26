Write-Host "Installing latest version of winget."

# Trust the PSGallery repository.
Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted

# Install the NuGet package provider
Write-Host " - installing NuGet package provider."
Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force -Scope CurrentUser | Out-Null

# Install the winget tools stuff.
Write-Host " - installing module wingettools"
Install-Module -Name WingetTools -Scope CurrentUser

Write-Host " - running `"Install-Winget`"."
Install-WinGet

# Upgrade winget to the newest version.
Write-Host " - upgrading winget to the latest version."
winget upgrade --all --silent --accept-package-agreements --accept-source-agreements --force