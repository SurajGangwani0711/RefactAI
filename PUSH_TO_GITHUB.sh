#!/bin/bash

# RefactAI - Push to GitHub Script
# This script helps you upload your project to GitHub

set -e

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║         RefactAI - GitHub Upload Helper                      ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# Check if git is initialized
if [ ! -d ".git" ]; then
    echo "❌ Error: Not a git repository!"
    echo "Run 'git init' first."
    exit 1
fi

# Check if already has remote
if git remote | grep -q "origin"; then
    echo "⚠️  Remote 'origin' already exists:"
    git remote -v
    echo ""
    read -p "Do you want to remove it and add a new one? (y/n) " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        git remote remove origin
        echo "✓ Removed existing remote"
    else
        echo "Keeping existing remote"
    fi
fi

# Ask for GitHub username
echo ""
echo "Enter your GitHub username:"
read -r GITHUB_USERNAME

if [ -z "$GITHUB_USERNAME" ]; then
    echo "❌ Error: GitHub username cannot be empty"
    exit 1
fi

# Ask for repository name
echo ""
echo "Enter repository name (press Enter for 'RefactAI'):"
read -r REPO_NAME

if [ -z "$REPO_NAME" ]; then
    REPO_NAME="RefactAI"
fi

# Construct repository URL
REPO_URL="https://github.com/${GITHUB_USERNAME}/${REPO_NAME}.git"

echo ""
echo "Repository URL: $REPO_URL"
echo ""
read -p "Is this correct? (y/n) " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 1
fi

# Add remote
echo ""
echo "Adding remote 'origin'..."
git remote add origin "$REPO_URL"
echo "✓ Remote added"

# Show status
echo ""
echo "Current branch:"
git branch --show-current

echo ""
echo "Ready to push!"
echo ""
read -p "Do you want to push now? (y/n) " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo ""
    echo "Pushing to GitHub..."
    git push -u origin main

    echo ""
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║  ✅ SUCCESS! Your project is now on GitHub!                  ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo ""
    echo "View your repository at:"
    echo "https://github.com/${GITHUB_USERNAME}/${REPO_NAME}"
    echo ""
    echo "Next steps:"
    echo "1. Add repository topics (ai, orleans, ollama, etc.)"
    echo "2. Add a description"
    echo "3. Enable Issues and Discussions"
    echo "4. Share with the world!"
else
    echo ""
    echo "To push manually later, run:"
    echo "git push -u origin main"
fi

echo ""
echo "Done!"
