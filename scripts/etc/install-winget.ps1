# Install winget
Install-Module -Name WingetTools
Install-WinGet
winget upgrade --all --silent --accept-package-agreements --accept-source-agreements --force