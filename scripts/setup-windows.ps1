# Before running this script, you will need to enter the following command:
# Set-RemoteExecutionPolicy RemoteSigned -Force

# Install winget
. $PSScriptRoot/etc/install-winget.ps1

# Install git
Write-Host "Installing git (latest version)"
winget install --id Git.Git -e --source winget

# Install unity hub
Write-Host "Installing UnityHub"
winget install -e Unity.UnityHub

# Get the version of the unity editor to install
$unityEditorVersion = (Get-Content $PSScriptRoot/../ProjectSettings/ProjectVersion.txt | 
    Where-Object { $_ -imatch 'm_EditorVersion' } |
    ForEach-Object { $_ -ireplace 'm_EditorVersion:\ '} |
    Select -First 1).Trim()

# Install unity editor version supported by the plugin
write-Host "Installing Unity Editor"
winget install ("Unity.Unity.{0}" -f $unityEditorVersion.Substring(0, 4)) -v $unityEditorVersion

Read-Host;

# TODO: Determine remaining pre-requisites for windows development
#  - What version of VS?
#  - Can we install unity editor via winget?
#  - Windows SDK?
#  - .NET SDK?
#  - .NET Runtime

