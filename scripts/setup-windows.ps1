# Before running this script, you will need to enter the following command:
# Set-RemoteExecutionPolicy RemoteSigned -Force

# Install winget
. ./etc/install-winget.ps1

# Install git
winget install --id Git.Git -e --source winget
# Install unity hub
winget install -e Unity.UnityHub



# TODO: Determine remaining pre-requisites for windows development
#  - What version of VS?
#  - Can we install unity editor via winget?
#  - Windows SDK?
#  - .NET SDK?
#  - .NET Runtime

