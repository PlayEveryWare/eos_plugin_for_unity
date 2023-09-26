# Before running this script, you will need to enter the following command:
# Set-RemoteExecutionPolicy RemoteSigned -Force

# Install winget
. $PSScriptRoot/etc/install-winget.ps1

# Install git
winget install --id Git.Git -e --source winget

# Install unity hub
winget install -e Unity.UnityHub

# Get the version of the unity editor to install
$unityEditorVersion = Get-Content $PSScriptRoot/../ProjectSettings/ProjectVersion.txt | 
    Where-Object { $_ -imatch 'm_EditorVersion' } |
    ForEach-Object { $_ -ireplace 'm_EditorVersion:\ '} |
    Select -First 1

winget install ("Unity.Unity.{0}" -f $unityEditorVersion.Substring(4)) -v $unityEditorVersion

Read-Host

# Install unity editor version supported by the plugin
#winget install Unity.Unity.2021 -v 2021.3.8f1





# TODO: Determine remaining pre-requisites for windows development
#  - What version of VS?
#  - Can we install unity editor via winget?
#  - Windows SDK?
#  - .NET SDK?
#  - .NET Runtime

