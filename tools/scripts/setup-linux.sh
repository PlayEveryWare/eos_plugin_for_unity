#!/bin/bash

# NOTE: The string below will need to be updated when the project is upgraded
# to support newer versions of the editor
UNITY_VERSION=2021.3.30f1

# Add source for git 
sudo add-apt-repository ppa:git-core/ppa -y

# Update apt-get 
sudo apt-get update

# Download and install the unity hub
sudo sh -c 'echo "deb https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'
wget -qO - https://hub.unity3d.com/linux/keys/public | sudo apt-key add -
sudo apt update
sudo apt install build-essential git unityhub libgbm-dev libasound2 libgconf-2-4 xvfb -y

# TODO: The following commands cause problems and need further experimentation
# Install Unity 2021.3 (for some reason cannot specify 2021.3.8f1)
sudo xvfb-run unityhub --headless install --version $UNITY_VERSION --no-sandbox