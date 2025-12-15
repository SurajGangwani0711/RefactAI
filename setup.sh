#!/bin/bash

# RefactAI Setup Script
# This script installs all dependencies and sets up the development environment

set -e  # Exit on error

echo "======================================"
echo "RefactAI Dependency Setup"
echo "======================================"
echo ""

# Check for .NET SDK
echo "Checking for .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found!"
    echo "Please install .NET 9.0 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET SDK found: $DOTNET_VERSION"
echo ""

# Check for Node.js
echo "Checking for Node.js..."
if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found!"
    echo "Please install Node.js from: https://nodejs.org/"
    exit 1
fi

NODE_VERSION=$(node --version)
echo "✅ Node.js found: $NODE_VERSION"
echo ""

# Check for Ollama
echo "Checking for Ollama..."
if ! command -v ollama &> /dev/null; then
    echo "⚠️  Ollama not found!"
    echo "Installing Ollama is recommended for AI-powered refactoring."
    echo "Install with: brew install ollama (macOS) or visit https://ollama.ai"
    echo ""
    read -p "Continue without Ollama? (y/n) " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    echo "✅ Ollama found"

    # Check for DeepSeek Coder model
    echo "Checking for deepseek-coder:6.7b model..."
    if ollama list | grep -q "deepseek-coder:6.7b"; then
        echo "✅ DeepSeek Coder model found"
    else
        echo "⚠️  DeepSeek Coder model not found"
        read -p "Do you want to pull the model now? This may take several minutes. (y/n) " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo "Pulling deepseek-coder:6.7b model..."
            ollama pull deepseek-coder:6.7b
            echo "✅ Model pulled successfully"
        fi
    fi
fi
echo ""

# Check for Git
echo "Checking for Git..."
if ! command -v git &> /dev/null; then
    echo "❌ Git not found!"
    echo "Please install Git from: https://git-scm.com/"
    exit 1
fi

GIT_VERSION=$(git --version)
echo "✅ Git found: $GIT_VERSION"
echo ""

# Restore .NET packages
echo "======================================"
echo "Restoring .NET packages..."
echo "======================================"
dotnet restore RefactAI.sln
echo "✅ .NET packages restored"
echo ""

# Install npm packages for UI
echo "======================================"
echo "Installing npm packages for UI..."
echo "======================================"
cd refactai-ui
npm install
cd ..
echo "✅ npm packages installed"
echo ""

# Create secrets directory if it doesn't exist
echo "Creating secrets directory..."
mkdir -p secrets
echo "✅ Secrets directory ready"
echo ""

# Create output directory
echo "Creating output directory..."
mkdir -p ~/RefactAI_Output
echo "✅ Output directory created at ~/RefactAI_Output"
echo ""

echo "======================================"
echo "✅ Setup Complete!"
echo "======================================"
echo ""
echo "Next steps:"
echo "1. Start the Orleans Silo: cd RefactAI.Orleans.Silo && dotnet run"
echo "2. Start the API: cd RefactAI.Api && dotnet run"
echo "3. Start the UI: cd refactai-ui && npm start"
echo ""
echo "Don't forget to:"
echo "- Create a GitHub Personal Access Token with 'repo' and 'workflow' scopes"
echo "- Configure the token and repository URL via the UI at http://localhost:3000"
echo ""
echo "For more information, see README.md"
