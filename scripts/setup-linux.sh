#!/bin/bash

# TODO: Check to make sure the distro is Ubuntu 18.04 (or whatever is currently supported)
# TODO: Test this script on a fresh install of 18.04 (or whatever is currently supported)

# Initial update and upgrade 
# TODO: this might be undesirable, remove if that's true
sudo apt-get update
sudo apt-get upgrade -y

# Install the build-essential package
sudo apt-get install build-essential -y

# Install the latest version of Git (needed for things like git lfs)
sudo add-apt-repository ppa:git-core/ppa
sudo apt update
sudo apt install git

# Install unity hub
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null
sudo sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'
sudo apt update
sudo apt-get install unityhub



