#!/bin/sh

# Make certain that we are running on a mac
if [[ "$(uname)" != "Darwin" ]]; then
    echo "This is NOT macOS. Please run the setup script for your platform."
fi

# Install brew
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Brewfile
BREWFILE="etc/Brewfile"

# If the brewfile exists
if [ -e "$BREWFILE" ]; then
    # Execute brewfile
    brew bundle --file="$BREWFILE"
else
    echo "Brewfile not found."
fi