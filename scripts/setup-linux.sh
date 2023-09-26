#!/bin/bash

# Add source for git 
sudo add-apt-repository ppa:git-core/ppa -y

# Update apt-get 
sudo apt-get update

# Install prereqs
sudo dnf install openssl1.1 openssl-libs

# Install packages needed for development
sudo apt install build-essential git -y

# Install Unity 2021.3 (for some reason cannot specify 2021.3.8f1)
# NOTE: The string below will need to be updated when the project is upgraded
# to support newer versions of the editor
# sudo unityhub --headless install --version 2021.3 --no-sandbox

sudo sh -c 'echo "deb https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'
wget -qO - https://hub.unity3d.com/linux/keys/public | sudo apt-key add -
sudo apt update
sudo apt install unityhub -y
sudo apt install libgbm-dev libasound2 libgconf-2-4 -y
sudo apt install xvfb -y

UNITY_VERSION=2021.3.30f1

sudo xvfb-run unityhub --headless install --version $UNITY_VERSION --no-sandbox

# Generate a license activation file (.alf)
sudo xvfb-run Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity -quit -batchmode -nographics -createManualActivationFile
# Get the license
sudo xvfb-run Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity -quit -batchmode -nographics -manualLicenseFile yourLicenseFile.ulf

sudo xvfb-run Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity -quit -batchmode -nographics -logFile - -projectPath ../ -buildTarget Linux64 -buildLinux64Player "Build.x86_64"