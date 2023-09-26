Write-Host "Installing Xaml package"

# Install prerequisite to winget
Invoke-WebRequest `
    -UseBasicParsing `
    -Uri "https://www.nuget.org/api/v2/package/Microsoft.UI.Xaml/2.7.0" `
    -OutFile Xaml.zip

Expand-Archive Xaml.zip

Add-AppxPackage .\Xaml\tools\AppX\x64\Release\Microsoft.UI.Xaml.2.7.appx

Remove-Item Xaml.zip
Remove-Item -Recurse -Force Xaml