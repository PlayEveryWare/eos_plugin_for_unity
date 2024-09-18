#!/bin/bash

# Get the current branch name
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

# Get the most recent tag by commit history (linear, not just nearest)
MOST_RECENT_TAG=$(git tag --sort=-creatordate | head -n 1)

# Check if a tag was found
if [ -z "$MOST_RECENT_TAG" ]; then
  echo "No tags found in the repository."
  exit 1
fi

# Print commit messages, hashes, and timestamps between the most recent tag and the current branch
echo "Commit messages between branch '$CURRENT_BRANCH' and the most recent tag '$MOST_RECENT_TAG':"
git log --pretty=format:"%cd - %an - %h - %s" --date=short $MOST_RECENT_TAG..$CURRENT_BRANCH
