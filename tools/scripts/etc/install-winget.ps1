Write-Information "Downloading WinGet and its dependencies..."

function Add-RemotePackage() {
    param($uri)
    $ErrorActionPreference = "SilentlyContinue"
    $ProgressPreference = 'SilentlyContinue'
    # display what we're doing
    Write-Host ("Downloading and installing package `"{0}`"" -f $uri)
    # temp file to store the package in
    $temp = New-TemporaryFile
    # download to temp file
    Invoke-WebRequest -Uri $uri -OutFile $temp -UseBasicParsing -Sile
    # add package
    Add-AppxPackage $temp -
    # remove package file
    Remove-Item $temp
    $ProgressPreference = 'Continue'
    $ErrorActionPreference = 'Continue'
}

Add-RemotePackage -uri https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx
Add-RemotePackage -uri https://github.com/microsoft/microsoft-ui-xaml/releases/download/v2.7.3/Microsoft.UI.Xaml.2.7.x64.appx
Add-RemotePackage -uri https://aka.ms/getwinget
