# RefactAI - Complete Guide

## 📖 Table of Contents

1. [What the Code Does](#what-the-code-does)
2. [Code Structure](#code-structure)
3. [How to Prepare to Run](#how-to-prepare-to-run)
4. [How to Run](#how-to-run)
5. [How to Use](#how-to-use)
6. [Troubleshooting](#troubleshooting)

---

## 1. What the Code Does

### 🎯 High-Level Purpose

RefactAI is an **automated code refactoring system** that uses AI to improve code quality across entire repositories. It:

1. **Connects to GitHub repositories** via webhooks or manual triggers
2. **Clones the repository** to analyze the code
3. **Processes each source file** using AI (DeepSeek Coder model via Ollama)
4. **Refactors the code** to improve quality while preserving functionality
5. **Creates a pull request** on GitHub with the improvements

### 🤖 What AI Refactoring Does

The AI automatically applies these improvements:

- ✅ **Clean Formatting**: Consistent indentation, spacing, and style
- ✅ **Remove Dead Code**: Unused variables, imports, and functions
- ✅ **Simplify Logic**: Replace complex code with clearer alternatives
- ✅ **Improve Naming**: Descriptive variable and function names
- ✅ **Best Practices**: Follow language-specific conventions
- ✅ **Preserve Behavior**: Keeps exact same functionality (no bugs introduced)

### 🌍 Supported Languages (12 Total)

The system can refactor code written in:

| Language | File Extensions |
|----------|----------------|
| C# | `.cs` |
| JavaScript | `.js` |
| TypeScript | `.ts` |
| Java | `.java` |
| Python | `.py` |
| Go | `.go` |
| Rust | `.rs` |
| C | `.c` |
| C++ | `.cpp`, `.cc` |
| Ruby | `.rb` |
| PHP | `.php` |
| Kotlin | `.kt` |

### 🔐 Security Features

- **Local-First Processing**: All AI inference runs on your machine (not cloud)
- **No Code Leakage**: Your source code never leaves your infrastructure
- **Token Security**: GitHub tokens stored locally, not in cloud services
- **Human Review**: All changes go through pull request review before merging

---

## 2. Code Structure

### 📁 Project Organization

```
RefactAI/
├── RefactAI.Api/                    # HTTP API (Entry Point)
│   ├── Controllers/
│   │   ├── WebhooksController.cs    # Handles GitHub webhooks & manual triggers
│   │   └── ConfigController.cs      # Manages GitHub token & repo configuration
│   └── Program.cs                   # API startup configuration
│
├── RefactAI.Orleans.Silo/           # Distributed Processing Engine
│   └── Program.cs                   # Hosts Orleans grains, registers services
│
├── RefactAI.Orleans.Contracts/      # Grain Interfaces (API contracts)
│   ├── RepoContracts.cs             # RepoGrain interface & data models
│   └── IPrGrain.cs                  # PrGrain interface
│
├── RefactAI.Orleans.Grains/         # Business Logic (Workers)
│   ├── RepoGrains.cs                # Router grain (validates & delegates)
│   ├── PrGrains.cs                  # Main worker (clone, refactor, PR)
│   └── GithubService.cs             # GitHub API integration
│
├── RefactAI.Common/                 # Shared Utilities
│   ├── AI/
│   │   ├── OllamaService.cs         # Local AI inference via Ollama
│   │   └── IOllamaService.cs        # Interface
│   ├── Runner/
│   │   └── DotnetRunner.cs          # Git operations (clone, commit, push)
│   ├── GlobalTokenStore.cs          # GitHub token storage
│   └── GlobalRepoStore.cs           # Repository URL storage
│
├── RefactAI.Refactor/               # Refactoring Logic
│   ├── RefactorService.cs           # Orchestrates AI refactoring
│   └── IRefactorService.cs          # Interface
│
├── RefactAI.AI/                     # AI Service (Placeholder for future)
│   └── AiRefactorService.cs         # Stub for Azure OpenAI integration
│
├── RefactAI.CodeAnalysis/           # Static Analysis
│   └── CodeAnalyzer.cs              # File/line counting, analysis
│
├── refactai-ui/                     # React Web Interface
│   ├── src/
│   │   ├── App.jsx                  # Main React app
│   │   ├── MainPage.jsx             # Configuration UI
│   │   └── pages/TokenManager.jsx   # Token management
│   └── package.json                 # npm dependencies
│
├── secrets/                         # Local Credentials (GITIGNORED)
│   ├── github.token                 # Your GitHub personal access token
│   └── Repo.url                     # Repository URL to refactor
│
└── Documentation/
    ├── README.md                    # Project overview
    ├── SYSTEM_FLOW_EXPLAINED.md     # Complete system flow
    ├── DEPENDENCIES.md              # All dependencies
    ├── HOW_IT_WORKS.md             # This file
    └── Presentations/               # PowerPoint decks
```

### 🏗️ Architecture Pattern: Microservices + Actor Model

RefactAI uses **Microsoft Orleans** (distributed actor framework) with these components:

#### **1. API Layer** (RefactAI.Api)
- **Role**: HTTP entry point
- **Port**: 5121
- **Responsibilities**:
  - Receives webhooks from GitHub
  - Provides configuration endpoints
  - Connects to Orleans Silo as a client

#### **2. Orleans Silo** (RefactAI.Orleans.Silo)
- **Role**: Processing engine
- **Ports**: 11111 (silo-to-silo), 30000 (gateway)
- **Responsibilities**:
  - Hosts grains (processing units)
  - Manages distributed state
  - Provides fault tolerance

#### **3. Grains** (RefactAI.Orleans.Grains)

**RepoGrain** - Lightweight Router
- One instance per repository URL
- Validates and normalizes requests
- Delegates to PrGrain

**PrGrain** - Heavy Worker
- One instance per repository URL
- Clones repository
- Processes files with AI
- Creates pull requests

#### **4. Services** (Injected via Dependency Injection)

| Service | Purpose | Location |
|---------|---------|----------|
| **OllamaService** | AI inference (DeepSeek Coder) | RefactAI.Common |
| **DotnetRunner** | Git operations | RefactAI.Common |
| **RefactorService** | Refactoring orchestration | RefactAI.Refactor |
| **GitHubService** | GitHub API (create PRs) | RefactAI.Orleans.Grains |

#### **5. Web UI** (refactai-ui)
- **Technology**: React 19.2.0
- **Port**: 3000
- **Purpose**: User-friendly configuration interface

---

## 3. How to Prepare to Run

### ✅ Prerequisites Checklist

Before running RefactAI, ensure you have:

- [ ] **.NET 9.0 SDK** installed
- [ ] **Node.js 18+** installed
- [ ] **Ollama** installed with DeepSeek Coder model
- [ ] **Git** installed
- [ ] **GitHub Personal Access Token** created

### 📦 Step-by-Step Setup

#### Step 1: Install .NET 9.0 SDK

**macOS:**
```bash
brew install dotnet
```

**Windows:**
Download from https://dotnet.microsoft.com/download

**Verify Installation:**
```bash
dotnet --version
# Should show: 9.0.x
```

---

#### Step 2: Install Node.js

**macOS:**
```bash
brew install node
```

**Windows:**
Download from https://nodejs.org/

**Verify Installation:**
```bash
node --version
# Should show: v18.x or higher

npm --version
# Should show: 8.x or higher
```

---

#### Step 3: Install Ollama

**macOS:**
```bash
brew install ollama
```

**Linux:**
```bash
curl -fsSL https://ollama.ai/install.sh | sh
```

**Windows:**
Download from https://ollama.ai/

**Verify Installation:**
```bash
ollama --version
```

---

#### Step 4: Download DeepSeek Coder Model

This is the AI model used for code refactoring (6.7 billion parameters).

```bash
ollama pull deepseek-coder:6.7b
```

**Expected Output:**
```
pulling manifest
pulling 8934d96d3f08... 100% ▕████████████▏ 3.8 GB
pulling 8c17c2ebb0ea... 100% ▕████████████▏ 7.0 KB
pulling 7c23fb36d801... 100% ▕████████████▏ 4.8 KB
pulling 2e0493f67d0c... 100% ▕████████████▏   59 B
pulling fa8235e5b48f... 100% ▕████████████▏  91 B
pulling 42347cd80dc8... 100% ▕████████████▏  486 B
verifying sha256 digest
writing manifest
success
```

**Verify Model:**
```bash
ollama list
```

You should see:
```
NAME                     ID              SIZE      MODIFIED
deepseek-coder:6.7b     8934d96d3f08    3.8 GB    2 minutes ago
```

---

#### Step 5: Create GitHub Personal Access Token

1. Go to https://github.com/settings/tokens
2. Click **"Generate new token (classic)"**
3. Give it a name: `RefactAI`
4. Select scopes:
   - ✅ **repo** (Full control of private repositories)
   - ✅ **workflow** (Update GitHub Action workflows)
5. Click **"Generate token"**
6. **Copy the token** (you won't see it again!)
   - Format: `ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`

---

#### Step 6: Clone RefactAI Repository

```bash
cd ~/Documents
git clone https://github.com/SurajGangwani0711/RefactAI.git
cd RefactAI
```

---

#### Step 7: Install Dependencies

**Automated Setup (Recommended):**
```bash
./setup.sh
```

This script will:
- Check all prerequisites
- Restore .NET packages
- Install npm packages
- Create necessary directories

**Manual Setup:**

```bash
# Restore .NET dependencies
dotnet restore RefactAI.sln

# Install UI dependencies
cd refactai-ui
npm install
cd ..

# Create secrets directory
mkdir -p secrets
```

---

#### Step 8: Configure GitHub Token

**Option A: Via File (Manual)**

```bash
# Save your GitHub token
echo "ghp_YOUR_TOKEN_HERE" > secrets/github.token

# Save repository URL (example)
echo "https://github.com/your-username/your-repo" > secrets/Repo.url
```

**Option B: Via Web UI (Recommended)**

You'll configure this after starting the application (see "How to Run" section).

---

### 🔍 Verify Setup

Run this checklist to ensure everything is ready:

```bash
# Check .NET
dotnet --version

# Check Node.js
node --version

# Check Ollama
ollama list | grep deepseek-coder

# Check Git
git --version

# Check dependencies restored
ls RefactAI.Api/bin/Debug/net9.0/ 2>/dev/null && echo "✓ .NET dependencies OK"

# Check UI dependencies
ls refactai-ui/node_modules/ 2>/dev/null && echo "✓ UI dependencies OK"
```

All checks should pass before proceeding.

---

## 4. How to Run

### 🚀 Quick Start (3 Terminals)

RefactAI requires **3 separate processes** running simultaneously:

1. **Orleans Silo** (Backend processing engine)
2. **API** (HTTP endpoints)
3. **Web UI** (React interface)

---

### Terminal 1: Start Orleans Silo

The Silo hosts the business logic (Grains).

```bash
cd RefactAI.Orleans.Silo
dotnet run
```

**Expected Output:**
```
info: Orleans.Runtime.SiloControl[0]
      Starting Silo
info: Orleans.Runtime.SiloControl[0]
      Silo is Ready
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Ports:**
- Silo-to-Silo: `11111`
- Gateway (API connects here): `30000`

**Leave this terminal running** ✅

---

### Terminal 2: Start API

The API receives webhooks and provides configuration endpoints.

```bash
cd RefactAI.Api
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5121
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**API Endpoints:**
- Swagger UI: http://localhost:5121
- Webhooks: http://localhost:5121/webhooks/run
- Config: http://localhost:5121/config/set-token

**Leave this terminal running** ✅

---

### Terminal 3: Start Web UI

The React UI provides a user-friendly interface for configuration.

```bash
cd refactai-ui
npm start
```

**Expected Output:**
```
Compiled successfully!

You can now view refactai-ui in the browser.

  Local:            http://localhost:3000
  On Your Network:  http://192.168.1.x:3000
```

Your browser will automatically open to http://localhost:3000

**Leave this terminal running** ✅

---

### ✅ Verify All Services Running

Open these URLs in your browser:

| Service | URL | Expected Result |
|---------|-----|-----------------|
| **Web UI** | http://localhost:3000 | RefactAI configuration page |
| **API Swagger** | http://localhost:5121 | Swagger documentation |
| **API Health** | http://localhost:5121/webhooks/run?repo=test | "Refactor started" message |

If all three work, you're ready to use RefactAI! 🎉

---

### 🛑 Stopping the Application

Press `Ctrl+C` in each of the 3 terminals to stop all services.

**Order doesn't matter**, but recommended:
1. Stop UI (Terminal 3)
2. Stop API (Terminal 2)
3. Stop Silo (Terminal 1)

---

### 🔄 Restarting

To restart RefactAI, simply run the same 3 commands again in 3 terminals.

---

## 5. How to Use

### 📋 Complete Workflow

#### Step 1: Configure GitHub Token (One-Time Setup)

**Via Web UI (Recommended):**

1. Open http://localhost:3000
2. Enter your GitHub Personal Access Token
3. Enter the repository URL you want to refactor
4. Click **"Save Token"** and **"Save Repo URL"**

**Via API (Alternative):**

```bash
# Set token
curl -X POST http://localhost:5121/config/set-token \
  -H "Content-Type: application/json" \
  -d '{"token": "ghp_YOUR_TOKEN_HERE"}'

# Set repository URL
curl -X POST http://localhost:5121/config/set-repo \
  -H "Content-Type: application/json" \
  -d '{"repoUrl": "https://github.com/your-username/your-repo"}'
```

**Verify Configuration:**

```bash
# Check token is saved
ls secrets/github.token

# Check repo URL is saved
ls secrets/Repo.url
```

---

#### Step 2: Trigger Refactoring

**Option A: Via Web UI**

1. Open http://localhost:3000
2. Click **"Launch Refactoring Pipeline"**
3. Wait for processing (you'll see logs in the Silo terminal)

**Option B: Via API (Manual Trigger)**

```bash
curl "http://localhost:5121/webhooks/run?repo=https://github.com/your-username/your-repo"
```

**Option C: Via GitHub Webhook (Automated)**

Configure a webhook in your GitHub repository:

1. Go to your repo → Settings → Webhooks → Add webhook
2. **Payload URL**: `http://your-server:5121/webhooks/github`
3. **Content type**: `application/json`
4. **Events**: Select "Pushes" and "Pull requests"
5. Click **"Add webhook"**

Now RefactAI will automatically process every push!

---

#### Step 3: Monitor Processing

**Watch Terminal Output:**

In the **Silo terminal**, you'll see:

```
info: RepoGrain[0] Received work for https://github.com/...
info: PrGrain[0] Starting processing for https://github.com/...
info: PrGrain[0] CLONING: https://github.com/... @ HEAD
info: PrGrain[0] CLONED TO: /tmp/refactai/abc123...
info: PrGrain[0] FOUND 42 files
info: PrGrain[0] Refactoring: /tmp/.../Program.cs (csharp)
info: PrGrain[0] ✓ Refactored: Program.cs
info: PrGrain[0] Refactoring: /tmp/.../Service.cs (csharp)
...
info: PrGrain[0] Commit and push to refact-ai-20231215143022
info: PrGrain[0] PR created: https://github.com/.../pull/42
info: PrGrain[0] ✓ Processing complete!
```

**Processing Time:**

| Repository Size | Estimated Time |
|----------------|----------------|
| Small (10 files) | 25-60 seconds |
| Medium (100 files) | 3-10 minutes |
| Large (1000 files) | 30-90 minutes |

*Time depends on file size and AI inference speed*

---

#### Step 4: Review the Pull Request

1. Go to your repository on GitHub
2. Click **"Pull requests"**
3. You'll see a new PR titled: `RefactorAI automated refactor (refact-ai-TIMESTAMP)`
4. Review the changes:
   - Click **"Files changed"** tab
   - Review each file's improvements
   - Check that behavior is preserved

**Example PR Description:**
```
## Automated Refactoring by RefactAI

This PR contains automated code improvements generated at 2023-12-15T14:30:22Z.

### Changes:
- Clean code formatting
- Dead code removal
- Logic simplification
- Improved naming conventions

⚠️ Please review carefully before merging.
```

---

#### Step 5: Merge or Request Changes

**If changes look good:**
1. Click **"Merge pull request"**
2. Confirm merge
3. Delete the branch (optional)

**If changes need adjustment:**
1. Leave review comments
2. Close the PR
3. Make manual fixes
4. Optionally re-run RefactAI

---

### 📊 Understanding the Output

#### Refactored Code Location

Refactored code is saved in two places:

1. **GitHub Pull Request** (for review and merging)
2. **Local Copy**: `~/RefactAI_Output/<repo-name>/<commit-sha>/`

**Example:**
```
~/RefactAI_Output/
  └── myrepo/
      └── HEAD/
          ├── src/
          │   ├── Program.cs
          │   └── Service.cs
          └── README.md
```

This local copy is useful for:
- Debugging
- Comparing before/after
- Offline review

---

### 🎨 Example: Before and After

**Before Refactoring:**

```csharp
// src/Program.cs
public void DoSomething(string x){
var y=x.Split(',');
for(int i=0;i<y.Length;i++){
Console.WriteLine(y[i]);
}
}
```

**After RefactAI:**

```csharp
// src/Program.cs
public void ProcessCommaSeparatedValues(string input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        return;
    }

    string[] values = input.Split(',');

    foreach (string value in values)
    {
        Console.WriteLine(value);
    }
}
```

**Improvements Applied:**
- ✅ Descriptive method name
- ✅ Input validation added
- ✅ Consistent formatting
- ✅ Modern `foreach` instead of `for`
- ✅ Better variable names
- ✅ Proper spacing and indentation

---

## 6. Troubleshooting

### 🔧 Common Issues and Solutions

#### Issue 1: "Ollama connection failed"

**Symptoms:**
```
error: OllamaService[0] Failed to connect to Ollama
```

**Solution:**

```bash
# Check if Ollama is running
ollama list

# If not running, start Ollama
ollama serve

# In another terminal, verify model is available
ollama list | grep deepseek-coder
```

**Expected Output:**
```
deepseek-coder:6.7b    3.8 GB    Available
```

---

#### Issue 2: "Orleans Silo won't start"

**Symptoms:**
```
error: Port 11111 is already in use
```

**Solution:**

```bash
# Find what's using port 11111
lsof -i :11111

# Kill the process
kill -9 <PID>

# Or use a different port (edit Program.cs in Orleans.Silo)
```

---

#### Issue 3: "GitHub API rate limit exceeded"

**Symptoms:**
```
error: GitHub API rate limit exceeded
```

**Solution:**

1. **Check rate limit status:**
```bash
curl -H "Authorization: token YOUR_TOKEN" \
  https://api.github.com/rate_limit
```

2. **Wait for reset** (shown in `reset` field)

3. **Use authenticated requests** (ensure token is configured)

4. **Increase limits**: GitHub Pro accounts have higher limits

---

#### Issue 4: "Git authentication failed"

**Symptoms:**
```
fatal: could not read Username for 'https://github.com'
```

**Solution:**

1. **Verify token is saved:**
```bash
cat secrets/github.token
```

2. **Ensure token has correct permissions:**
   - Go to https://github.com/settings/tokens
   - Check that `repo` and `workflow` scopes are enabled

3. **Regenerate token if necessary**

---

#### Issue 5: "API can't connect to Silo"

**Symptoms:**
```
error: Failed to connect to Orleans gateway at localhost:30000
```

**Solution:**

1. **Ensure Silo is running first** (Terminal 1)
2. **Check Silo is listening on port 30000:**
```bash
lsof -i :30000
```

3. **Restart Silo and API in order:**
   - Stop both
   - Start Silo first
   - Wait for "Silo is Ready"
   - Then start API

---

#### Issue 6: "UI shows 'Failed to fetch'"

**Symptoms:**
Web UI displays error when trying to configure.

**Solution:**

1. **Verify API is running:**
```bash
curl http://localhost:5121/config/get-token
```

2. **Check CORS settings** in `RefactAI.Api/Program.cs`

3. **Restart API and UI**

---

#### Issue 7: "No changes to commit"

**Symptoms:**
```
info: PrGrain[0] No changes to commit
```

**Possible Causes:**

1. **Code is already well-formatted** - AI found no improvements needed
2. **Unsupported language** - All files were skipped
3. **AI returned identical code** - Model didn't suggest changes

**Not necessarily an error!** This means your code is already good.

---

#### Issue 8: "File too large for AI"

**Symptoms:**
```
warn: RefactorService[0] File too large, skipping: Program.cs
```

**Solution:**

Ollama has context limits (~8000 tokens). For very large files:

1. **Split large files** into smaller modules
2. **Increase Ollama context** (advanced):
```bash
ollama run deepseek-coder:6.7b --ctx 16000
```

3. **Skip problematic files** (they'll remain unchanged)

---

### 🔍 Debugging Tips

#### Enable Verbose Logging

**In Orleans Silo:**

Edit `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Orleans": "Debug"
    }
  }
}
```

**In API:**

Same edit in `RefactAI.Api/appsettings.json`

---

#### Check Logs

**Silo Logs:**
- Displayed in Terminal 1
- Shows grain activations, processing steps

**API Logs:**
- Displayed in Terminal 2
- Shows HTTP requests, Orleans client calls

**UI Logs:**
- Displayed in Terminal 3
- Shows React component updates, API calls

---

#### Test Individual Components

**Test Ollama Directly:**

```bash
ollama run deepseek-coder:6.7b
```

Then type a prompt:
```
Refactor this code to use modern C# syntax: void foo(){int x=5;}
```

**Test GitHub API:**

```bash
curl -H "Authorization: token YOUR_TOKEN" \
  https://api.github.com/user
```

Should return your GitHub user info.

**Test Git Clone:**

```bash
cd /tmp
git clone https://YOUR_TOKEN@github.com/your-username/your-repo.git
```

---

### 📞 Getting Help

If you're still stuck:

1. **Check documentation:**
   - `README.md` - Project overview
   - `SYSTEM_FLOW_EXPLAINED.md` - Detailed flow
   - `DEPENDENCIES.md` - All dependencies

2. **Review logs** in all 3 terminals

3. **Search GitHub Issues**: https://github.com/SurajGangwani0711/RefactAI/issues

4. **Create new issue** with:
   - Error message
   - Steps to reproduce
   - Log output
   - System info (OS, .NET version, etc.)

---

## 🎉 Success Checklist

You're successfully running RefactAI when:

- [ ] All 3 terminals are running without errors
- [ ] Web UI loads at http://localhost:3000
- [ ] API Swagger loads at http://localhost:5121
- [ ] Ollama responds to `ollama list`
- [ ] GitHub token is saved in `secrets/github.token`
- [ ] You can trigger refactoring via UI or API
- [ ] Pull request is created on GitHub
- [ ] Refactored code is visible in PR

---

## 📚 Additional Resources

### Documentation Files

- **README.md** - Project overview and quick start
- **SYSTEM_FLOW_EXPLAINED.md** - Complete system architecture
- **DEPENDENCIES.md** - All dependencies with versions
- **GITHUB_SETUP.md** - How to upload to GitHub
- **HOW_IT_WORKS.md** - This file

### Presentations

- **RefactAI_Presentation.pptx** - 53-slide technical deck
- **RefactAI_Pitch_Deck.pptx** - 10-slide investor pitch
- **presentation.html** - Interactive web presentation

### External Links

- **Microsoft Orleans**: https://learn.microsoft.com/en-us/dotnet/orleans/
- **Ollama**: https://ollama.ai/
- **DeepSeek Coder**: https://github.com/deepseek-ai/DeepSeek-Coder
- **GitHub API**: https://docs.github.com/en/rest

---

## 🚀 Quick Reference Commands

```bash
# Setup
./setup.sh                              # Automated setup

# Run (3 terminals)
cd RefactAI.Orleans.Silo && dotnet run  # Terminal 1
cd RefactAI.Api && dotnet run           # Terminal 2
cd refactai-ui && npm start             # Terminal 3

# Configure
curl -X POST http://localhost:5121/config/set-token \
  -H "Content-Type: application/json" \
  -d '{"token": "YOUR_TOKEN"}'

# Trigger
curl "http://localhost:5121/webhooks/run?repo=https://github.com/user/repo"

# Check status
ollama list                             # Verify AI model
git status                              # Check repo status
curl http://localhost:5121              # Check API
```

---

**You're now ready to use RefactAI to improve code quality automatically! 🎉**

For questions or issues, see the Troubleshooting section or create a GitHub issue.
