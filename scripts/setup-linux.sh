#!/bin/bash

# Add source for git 
sudo add-apt-repository ppa:git-core/ppa -y

# Add source for unity hub
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null
sudo sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'

# Update apt-get 
sudo apt-get update

# Install prereqs
sudo dnf install openssl1.1 openssl-libs

# Install packages needed for development
sudo apt install build-essential git unityhub xvfb -y

# Install Unity 2021.3 (for some reason cannot specify 2021.3.8f1)
# NOTE: The string below will need to be updated when the project is upgraded
# to support newer versions of the editor
unityhub --headless install --version 2021.3
