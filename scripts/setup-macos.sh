#!/bin/bash

# Brewfile
BREWFILE="etc/Brewfile"

if [ -e "$BREWFILE" ]; then
    # Execute brewfile
    brew bundle --file="$BREWFILE"
else
    echo "Brewfile not found."
fi