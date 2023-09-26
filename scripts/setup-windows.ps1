# Before running this script, you will need to enter the following command:
# Set-RemoteExecutionPolicy RemoteSigned -Force

# Install winget
. $PSScriptRoot/etc/install-winget.ps1

# Install visual studio community edition 2019
winget install --id XP8CDJNZKFM06W --accept-source-agreements --accept-package-agreements --override "--quiet"

# Install git
Write-Host "Installing git (latest version)"
winget install --id Git.Git -e --source winget

# Install unity hub
Write-Host "Installing UnityHub"
winget install -e Unity.UnityHub --accept-source-agreements

# Get the version of the unity editor to install from the project version
$unityEditorVersion = (Get-Content $PSScriptRoot/../ProjectSettings/ProjectVersion.txt | 
    Where-Object { $_ -imatch 'm_EditorVersion' } |
    ForEach-Object { $_ -ireplace 'm_EditorVersion:\ '} |
    Select -First 1).Trim()

# Install unity editor version supported by the plugin
write-Host "Installing Unity Editor"
winget install ("Unity.Unity.{0}" -f $unityEditorVersion.Substring(0, 4)) -v $unityEditorVersion --accept-source-agreements

# Install visual studio community edition 2019
winget install --id XP8CDJNZKFM06W --accept-source-agreements --override "--quiet"