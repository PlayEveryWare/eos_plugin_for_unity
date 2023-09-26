#!/bin/bash

# TODO: Check to make sure the distro is Ubuntu 18.04 (or whatever is currently supported)
# TODO: Test this script on a fresh install of 18.04 (or whatever is currently supported)

# Add source for git 
sudo add-apt-repository ppa:git-core/ppa -y
# Add source for unity hub
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null
sudo sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'

# Update apt-get 
sudo apt-get update

# Install the build-essential package
sudo apt install build-essential -y

# Install the latest version of Git (needed for things like git lfs)
sudo apt install git -y

# Install unity hub
sudo apt-get install unityhub -y



