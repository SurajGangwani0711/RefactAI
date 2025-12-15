# RefactAI

An AI-powered automated code refactoring system that integrates with GitHub repositories to analyze, refactor, and create pull requests with improved code. Built with Microsoft Orleans for distributed processing and Ollama for local AI-driven code improvements.

## Features

- **Automated Code Refactoring**: AI-driven code improvements using DeepSeek Coder model
- **Multi-Language Support**: C#, JavaScript, TypeScript, Java, Python, C/C++, Kotlin, Go, Rust, Ruby, PHP
- **GitHub Integration**: Webhook-triggered processing with automated PR creation
- **Distributed Architecture**: Microsoft Orleans for scalable, fault-tolerant processing
- **Local-First Security**: Runs entirely on your local machine with secure token storage
- **Web UI**: Easy configuration and monitoring interface

## Architecture

RefactAI uses a microservices architecture built on Microsoft Orleans:

```
GitHub Repo → Webhooks → API → Orleans Client → Orleans Silo → Grains
   ↓                                                    ↓
   ↓                                              Process & Refactor
   ↓                                                    ↓
   ←─────────────── Automated Pull Request ←────────────
```

### System Components

- **RefactAI.Api**: RESTful API gateway with webhook endpoints (Port 5121)
- **RefactAI.Orleans.Silo**: Orleans cluster host for distributed processing
- **RefactAI.Orleans.Grains**: Business logic for repository and PR processing
- **RefactAI.Refactor**: Core refactoring logic with Ollama integration
- **RefactAI.Common**: Shared utilities, Git operations, and AI services
- **refactai-ui**: React-based web interface for configuration

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) (for UI)
- [Ollama](https://ollama.ai/) with DeepSeek Coder model
- GitHub Personal Access Token with repository access

## Installation

### 1. Install Ollama and DeepSeek Coder Model

```bash
# Install Ollama (macOS)
brew install ollama

# Pull the DeepSeek Coder model
ollama pull deepseek-coder:6.7b
```

### 2. Clone the Repository

```bash
git clone <your-repo-url>
cd RefactAI
```

### 3. Build the Solution

```bash
dotnet build RefactAI.sln
```

### 4. Install UI Dependencies

```bash
cd refactai-ui
npm install
```

## Configuration

### GitHub Token Setup

1. Create a [GitHub Personal Access Token](https://github.com/settings/tokens) with the following permissions:
   - `repo` (Full control of private repositories)
   - `workflow` (Update GitHub Action workflows)

2. The token will be stored securely at: `/Users/suraj/Documents/Personal_project/RefactAI/secrets/github.token`

## Running the Application

### Start the Orleans Silo

```bash
cd RefactAI.Orleans.Silo
dotnet run
```

The silo will start on:
- Main port: 11111
- Gateway port: 30000

### Start the API

```bash
cd RefactAI.Api
dotnet run
```

The API will be available at: `http://localhost:5121`

Swagger documentation: `http://localhost:5121`

### Start the UI

```bash
cd refactai-ui
npm start
```

The UI will be available at: `http://localhost:3000`

## Usage

### Via Web UI

1. Navigate to `http://localhost:3000`
2. Enter your GitHub Personal Access Token
3. Enter the repository URL you want to refactor
4. Click "Launch Refactoring Pipeline"

### Via API

#### Manual Trigger

```bash
curl "http://localhost:5121/webhooks/run?repo=https://github.com/owner/repo"
```

#### Set Configuration

```bash
# Set GitHub token
curl -X POST http://localhost:5121/config/set-token \
  -H "Content-Type: application/json" \
  -d '{"token": "your-github-token"}'

# Set repository URL
curl -X POST http://localhost:5121/config/set-repo \
  -H "Content-Type: application/json" \
  -d '{"repoUrl": "https://github.com/owner/repo"}'
```

#### GitHub Webhook

Configure a webhook in your GitHub repository:
- **Payload URL**: `http://your-server:5121/webhooks/github`
- **Content type**: `application/json`
- **Events**: Push events, Pull request events

## How It Works

1. **Configuration**: User provides GitHub token and repository URL via the React UI or API
2. **Trigger**: Webhook received from GitHub (or manual trigger via API)
3. **Processing**:
   - API forwards request to Orleans `RepoGrain`
   - `RepoGrain` normalizes URL and dispatches to `PrGrain`
   - `PrGrain` orchestrates the refactoring workflow:
     - Clones the repository to a temporary directory
     - Processes each source file
     - Calls Ollama for AI-based refactoring suggestions
     - Commits changes to a new branch
     - Pushes branch to GitHub
     - Creates a pull request via GitHub API
4. **Output**: Refactored code saved locally and PR created on GitHub

## API Endpoints

### Webhooks

- `GET /webhooks/run?repo=<url>` - Manually trigger refactoring for a repository
- `POST /webhooks/github` - GitHub webhook endpoint (push/PR events)

### Configuration

- `POST /config/set-token` - Save GitHub token
- `POST /config/set-repo` - Save repository URL
- `GET /config/get-token` - Retrieve stored token
- `GET /config/get-repo` - Retrieve stored repository URL

## Refactoring Process

The AI refactoring service applies the following improvements:

- Clean and consistent code formatting
- Remove dead code and unused imports
- Simplify complex logic
- Improve variable and function naming
- Maintain exact code behavior (no functional changes)

## Output

Refactored code is saved to: `/Users/suraj/RefactAI_Output/`

Pull requests are created on GitHub with detailed descriptions of the changes.

## Technology Stack

### Backend
- .NET 9.0
- ASP.NET Core Web API
- Microsoft Orleans 9.2.1
- Swashbuckle (Swagger/OpenAPI)
- Octokit 14.0.0 (GitHub API)
- Polly 8.6.4 (Resilience)

### Frontend
- React 19.2.0
- React Router 7.10.0
- React Scripts 5.0.1

### AI/ML
- Ollama (Local LLM inference)
- DeepSeek Coder 6.7B model

### DevOps
- Git CLI
- GitHub REST API

## Project Structure

```
RefactAI/
├── RefactAI.Api/              # RESTful API gateway
├── RefactAI.AI/               # AI refactoring service
├── RefactAI.CodeAnalysis/     # Static code analysis
├── RefactAI.Common/           # Shared utilities and services
├── RefactAI.Orleans.Contracts/# Orleans grain interfaces
├── RefactAI.Orleans.Grains/   # Orleans grain implementations
├── RefactAI.Orleans.Silo/     # Orleans cluster host
├── RefactAI.Refactor/         # Core refactoring logic
├── RefactAI.Workers/          # Background services
├── refactai-ui/               # React web interface
├── secrets/                   # Local token storage
└── RefactAI.sln               # Solution file
```

## Development

### Build

```bash
dotnet build RefactAI.sln
```

### Run Tests

```bash
dotnet test RefactAI.Tests/RefactAI.Tests.csproj
```

### Clean

```bash
dotnet clean RefactAI.sln
```

## Security

- GitHub tokens are stored locally in the `secrets/` directory
- No cloud dependencies - runs entirely on your local machine
- Token-based authentication for GitHub API access
- HTTPS is used for all GitHub operations

## Troubleshooting

### Ollama Connection Issues

Ensure Ollama is running:
```bash
ollama serve
```

Verify the model is available:
```bash
ollama list
```

### GitHub Authentication Errors

- Verify your Personal Access Token has the correct permissions
- Check that the token is properly saved via the configuration endpoints
- Ensure the repository URL is accessible with your token

### Orleans Silo Connection Issues

- Verify the silo is running on port 11111
- Check firewall settings allow local connections
- Review silo logs for any startup errors

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

[Specify your license here]

## Support

For issues and questions, please open an issue on GitHub.

## Roadmap

- [ ] Integration with Azure OpenAI for enhanced refactoring
- [ ] Support for more programming languages
- [ ] Advanced code analysis features
- [ ] Customizable refactoring rules
- [ ] Real-time progress monitoring in UI
- [ ] Docker containerization
- [ ] Cloud deployment options
