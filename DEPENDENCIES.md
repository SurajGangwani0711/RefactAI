# Project Dependencies

This document lists all dependencies for the RefactAI project.

## .NET Dependencies

### Runtime
- **.NET SDK**: 9.0

### NuGet Packages

#### RefactAI.Api
- Microsoft.AspNetCore.OpenApi: 9.0.9
- Microsoft.Orleans.Client: 9.2.1
- Microsoft.Orleans.Core.Abstractions: 9.2.1
- Newtonsoft.Json: 13.0.4
- Octokit: 14.0.0
- OpenTelemetry.Extensions.Hosting: 1.13.1
- Polly: 8.6.4
- Swashbuckle.AspNetCore: 6.5.0

#### RefactAI.Orleans.Silo
- Microsoft.Orleans.Core.Abstractions: 9.2.1
- Microsoft.Orleans.Server: 9.2.1
- Microsoft.Extensions.Http: 8.0.0

#### RefactAI.Orleans.Grains
- Microsoft.Orleans.Core.Abstractions: 9.2.1
- System.Threading.Tasks.Dataflow: 9.0.10

#### RefactAI.Orleans.Contracts
- Microsoft.Orleans.Core.Abstractions: 9.2.1

#### RefactAI.Workers
- Microsoft.Extensions.Hosting: 9.0.9
- System.Threading.Channels: 9.0.10
- System.Threading.Tasks.Dataflow: 9.0.10

#### RefactAI.AI
- Microsoft.ML: 4.0.2
- Microsoft.ML.OnnxRuntime: 1.23.2

#### RefactAI.Refactor
- Microsoft.Extensions.Logging.Abstractions: 10.0.0

#### RefactAI.RefactorAgent
- Microsoft.Build.Locator: 1.10.2
- Microsoft.CodeAnalysis.CSharp.Workspaces: 4.10.0
- Microsoft.CodeAnalysis.Workspaces.MSBuild: 4.10.0
- Microsoft.Extensions.Logging: 8.0.0

#### RefactAI.CodeAnalysis
- Microsoft.CodeAnalysis.CSharp.Workspaces: 4.14.0

#### RefactAI.Refactorings
- Microsoft.CodeAnalysis.CSharp.Workspaces: 4.14.0

#### RefactAI.Tests
- coverlet.collector: 6.0.2
- Microsoft.NET.Test.Sdk: 17.12.0
- xunit: 2.9.2
- xunit.runner.visualstudio: 2.8.2

## Frontend Dependencies (refactai-ui)

### Runtime
- **Node.js**: 18+ (recommended: LTS version)
- **npm**: 8+

### npm Packages

#### Production Dependencies
- react: ^19.2.0
- react-dom: ^19.2.0
- react-router-dom: ^7.10.0
- react-scripts: 5.0.1
- web-vitals: ^2.1.4

#### Development Dependencies
- @testing-library/dom: ^10.4.1
- @testing-library/jest-dom: ^6.9.1
- @testing-library/react: ^16.3.0
- @testing-library/user-event: ^13.5.0

## External Services

### Required
- **Ollama**: Local LLM runtime
  - Model: `deepseek-coder:6.7b`
  - Installation: `brew install ollama` (macOS) or see https://ollama.ai
  - Pull model: `ollama pull deepseek-coder:6.7b`

- **Git**: Version control (usually pre-installed on macOS/Linux)
  - Installation: `brew install git` (macOS) or see https://git-scm.com

### Optional
- **GitHub Personal Access Token**: Required for GitHub integration
  - Scopes needed: `repo`, `workflow`
  - Create at: https://github.com/settings/tokens

## System Requirements

### Minimum
- **OS**: macOS 10.15+, Linux (Ubuntu 20.04+), Windows 10+
- **RAM**: 8 GB
- **Disk Space**: 10 GB free (for Ollama models and temporary repositories)
- **CPU**: x64 or ARM64 architecture

### Recommended
- **RAM**: 16 GB or more (for better Ollama performance)
- **SSD**: For faster repository cloning and file operations

## Installation Commands

### Install .NET SDK
```bash
# macOS
brew install dotnet

# Or download from: https://dotnet.microsoft.com/download
```

### Install Node.js
```bash
# macOS
brew install node

# Or download from: https://nodejs.org/
```

### Install Ollama
```bash
# macOS
brew install ollama

# Linux
curl -fsSL https://ollama.ai/install.sh | sh

# Windows: Download from https://ollama.ai
```

### Pull Ollama Model
```bash
ollama pull deepseek-coder:6.7b
```

### Restore .NET Packages
```bash
dotnet restore RefactAI.sln
```

### Install npm Packages
```bash
cd refactai-ui
npm install
```

## Dependency Management

### Updating .NET Packages
```bash
# Update all packages to latest compatible versions
dotnet list package --outdated

# Update specific package
dotnet add package <PackageName> --version <Version>
```

### Updating npm Packages
```bash
cd refactai-ui

# Check for outdated packages
npm outdated

# Update packages
npm update

# Update to latest major versions (use with caution)
npm install <package>@latest
```

## Version Compatibility

- All .NET projects target **.NET 9.0**
- Orleans packages should remain at the same version (9.2.1)
- Microsoft.CodeAnalysis packages should remain compatible with the .NET SDK version
- React and related packages should maintain compatibility with React 19.x

## License Information

Refer to individual package licenses for compliance requirements:
- Most Microsoft packages: MIT License
- React: MIT License
- Ollama: MIT License
- Check individual package licenses using: `dotnet list package --include-transitive`
