# RefactAI System Flow - Complete Explanation

## 📚 Table of Contents

1. [What is Orleans Silo?](#what-is-orleans-silo)
2. [Complete System Architecture](#complete-system-architecture)
3. [Step-by-Step Flow](#step-by-step-flow)
4. [Component Deep Dive](#component-deep-dive)
5. [Code Examples](#code-examples)
6. [Execution Timeline](#execution-timeline)

---

## What is Orleans Silo?

### 🎯 Purpose

The **Orleans Silo** is the **backend processing engine** of RefactAI. Think of it as a **distributed actor system** that:

- **Hosts Grains** (isolated processing units)
- **Manages State** (each grain has its own state)
- **Handles Concurrency** (automatic thread-safe execution)
- **Provides Scalability** (can run on multiple machines)
- **Ensures Fault Tolerance** (auto-recovery from failures)

### 📍 Location in Your Project

```
RefactAI.Orleans.Silo/
├── Program.cs                 # Main entry point
└── RefactAI.Orleans.Silo.csproj
```

### 🔧 What It Does

```csharp
// RefactAI.Orleans.Silo/Program.cs

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // 1. Register all services that Grains need
        services.AddHttpClient();
        services.AddSingleton<IGitHubService, GitHubService>();
        services.AddSingleton<IOllamaService, OllamaService>();
        services.AddSingleton<IDotnetRunner, DotnetRunner>();
        services.AddSingleton<IRefactorService, RefactorService>();
    })
    .UseOrleans(silo =>
    {
        // 2. Configure Orleans to run locally
        silo.UseLocalhostClustering(
            siloPort: 11111,        // Silo-to-silo communication
            gatewayPort: 30000);    // Client-to-silo communication

        silo.Configure<ClusterOptions>(opts =>
        {
            opts.ClusterId = "default";
            opts.ServiceId = "default";
        });
    });

// 3. Start the silo and keep it running
await builder.Build().RunAsync();
```

### 🌟 Key Concepts

#### Grain
- A **Grain** is like a **microservice** with its own state
- Each grain has a **unique identity** (key)
- Orleans ensures **only one instance** of each grain exists
- Grains are **automatically thread-safe**

**Example:**
```csharp
// Get a grain for a specific repository
var repoGrain = cluster.GetGrain<IRepoGrain>("https://github.com/user/repo");

// Orleans guarantees:
// - Only one RepoGrain exists for this URL
// - All calls to it are serialized (no race conditions)
// - It can be on any machine in the cluster
```

#### Silo vs Client

| Component | Role | Location |
|-----------|------|----------|
| **Silo** | Hosts and executes grains | RefactAI.Orleans.Silo |
| **Client** | Connects to silo and calls grains | RefactAI.Api |

```
┌──────────────┐           ┌──────────────┐
│ API (Client) │──────────▶│ Silo (Host)  │
│              │  Gateway  │              │
│ GetGrain()   │  :30000   │ Grains run   │
└──────────────┘           └──────────────┘
```

---

## Complete System Architecture

### 🏗️ High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         USER / GITHUB                                │
│  (Web UI or GitHub Webhook)                                         │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               │ HTTP Request
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      RefactAI.Api (Port 5121)                        │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ WebhooksController                                             │ │
│  │ • GET  /webhooks/run?repo=...                                  │ │
│  │ • POST /webhooks/github                                        │ │
│  │                                                                │ │
│  │ ConfigController                                               │ │
│  │ • POST /config/set-token                                       │ │
│  │ • POST /config/set-repo                                        │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                               │                                      │
│                               │ Orleans Client                       │
│                               │ (Gateway :30000)                     │
└───────────────────────────────┼──────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│            Orleans Silo (Port 11111, Gateway 30000)                  │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ RepoGrain (Router)                                             │ │
│  │ • One per repository URL                                       │ │
│  │ • Validates and normalizes requests                            │ │
│  │ • Delegates to PrGrain                                         │ │
│  └─────────────────────┬──────────────────────────────────────────┘ │
│                        │                                             │
│                        ▼                                             │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ PrGrain (Worker)                                               │ │
│  │ • One per repository URL                                       │ │
│  │ • Orchestrates entire refactoring workflow                     │ │
│  │ • Uses injected services                                       │ │
│  └─┬────────────┬────────────┬─────────────────────────────────────┘ │
│    │            │            │                                       │
└────┼────────────┼────────────┼───────────────────────────────────────┘
     │            │            │
     │            │            │
     ▼            ▼            ▼
┌──────────┐ ┌─────────┐ ┌──────────────┐
│ Dotnet   │ │ Refactor│ │   GitHub     │
│ Runner   │ │ Service │ │   Service    │
│          │ │         │ │              │
│ Git Ops  │ │ AI Ops  │ │  PR Creation │
└────┬─────┘ └────┬────┘ └──────┬───────┘
     │            │             │
     ▼            ▼             ▼
┌──────────┐ ┌─────────┐ ┌──────────────┐
│   Git    │ │ Ollama  │ │  GitHub API  │
│ Commands │ │ AI Model│ │              │
└──────────┘ └─────────┘ └──────────────┘
```

---

## Step-by-Step Flow

### 🎬 Scenario: User Triggers Refactoring

#### Step 1: User Configures GitHub Token (One-Time)

**Location:** RefactAI.Api/Controllers/ConfigController.cs

```
User → Web UI → POST /config/set-token
                 POST /config/set-repo
```

```csharp
[HttpPost("set-token")]
public async Task<IActionResult> SetToken([FromBody] TokenUpdateRequest req)
{
    // Save token to local file
    GlobalTokenStore.Save(req.Token);

    // Path: /Users/suraj/.../RefactAI/secrets/github.token

    return Ok("Token saved");
}
```

**What Happens:**
- Token saved to `secrets/github.token`
- Repo URL saved to `secrets/Repo.url`
- These are loaded later for Git and GitHub API operations

---

#### Step 2: User Triggers Refactoring

**Location:** RefactAI.Api/Controllers/WebhooksController.cs

```
User → Web UI → GET /webhooks/run?repo=https://github.com/user/myrepo
```

```csharp
[HttpGet("run")]
public async Task<IActionResult> Run([FromQuery] string repo)
{
    // 1. Clean up URL (remove /blob/, /tree/ paths)
    string normalized = NormalizeRepoUrl(repo);
    // Input:  "https://github.com/user/repo/blob/main/file.cs"
    // Output: "https://github.com/user/repo"

    // 2. Get or create RepoGrain
    var grain = _cluster.GetGrain<IRepoGrain>(normalized);

    // 3. Enqueue work
    await grain.EnqueueWork(new RepoWorkItem(
        RepoUrl: normalized,
        Branch: "main",
        Sha: "HEAD",
        Kind: "PR"
    ));

    return Ok($"Refactor started for {normalized}");
}
```

**What Happens:**
1. API receives HTTP request
2. URL normalized (removes extra paths)
3. **Orleans Client** connects to **Silo** (port 30000)
4. Client asks Silo for `RepoGrain` with key = repo URL
5. Silo creates or activates the grain
6. Work item enqueued
7. API returns immediately (async processing continues in background)

---

#### Step 3: RepoGrain Routes the Work

**Location:** RefactAI.Orleans.Grains/RepoGrains.cs

```
API → RepoGrain.EnqueueWork(RepoWorkItem)
```

```csharp
public class RepoGrain : Grain, IRepoGrain
{
    public async Task EnqueueWork(RepoWorkItem item)
    {
        _logger.LogInformation($"[RepoGrain] Received work for {item.RepoUrl}");

        // Normalize URL again (defensive)
        string normalized = NormalizeRepoUrl(item.RepoUrl);

        // Get PrGrain (keyed by same repo URL)
        var prGrain = GrainFactory.GetGrain<IPrGrain>(normalized);

        // Create processing request
        var request = new PrRequest(
            RepoUrl: normalized,
            Sha: item.Sha ?? "HEAD",
            PrNumber: item.Kind
        );

        // Delegate to PrGrain
        await prGrain.ProcessPr(request);

        _logger.LogInformation($"[RepoGrain] Delegated to PrGrain");
    }
}
```

**What Happens:**
1. RepoGrain receives work item
2. Validates and normalizes data
3. Gets or creates `PrGrain` for the same repository
4. Forwards request to PrGrain
5. Acts as a **router** between API and processing logic

**Why Two Grains?**
- **RepoGrain**: Lightweight router (stable interface)
- **PrGrain**: Heavy worker (complex logic, long-running)
- Separation of concerns: routing vs processing

---

#### Step 4: PrGrain Starts Processing

**Location:** RefactAI.Orleans.Grains/PrGrains.cs

```
RepoGrain → PrGrain.ProcessPr(PrRequest)
```

```csharp
public Task<PrResult> ProcessPr(PrRequest request)
{
    _logger.LogInformation($"[PrGrain] Starting processing for {request.RepoUrl}");

    // Schedule async processing using Orleans timer
    RegisterTimer(
        async _ => await ProcessInternalAsync(request),
        null,
        TimeSpan.Zero,              // Start immediately
        TimeSpan.FromMilliseconds(-1)  // Run once (not recurring)
    );

    // Return immediately (non-blocking)
    return Task.FromResult(
        new PrResult("Started", $"Processing {request.RepoUrl}")
    );
}
```

**What Happens:**
1. PrGrain receives request
2. Registers an **Orleans Timer** to run `ProcessInternalAsync`
3. Returns immediately (grain is not blocked)
4. Timer fires → actual processing begins

**Why Use a Timer?**
- Grains should return quickly (avoid blocking)
- Long-running work (git clone, AI inference) happens in timer callback
- Orleans runtime manages the timer lifecycle

---

#### Step 5: Clone Repository

```
PrGrain → DotnetRunner.CloneRepo(repoUrl, sha)
```

```csharp
private async Task ProcessInternalAsync(PrRequest request)
{
    _logger.LogInformation("[PrGrain] Step 1: Clone repository");

    // Clone to temp directory
    var clonePath = await _runner.CloneRepo(request.RepoUrl, request.Sha);
    // Example: /tmp/refactai/abc123-def456-...

    _logger.LogInformation($"[PrGrain] Cloned to: {clonePath}");
```

**DotnetRunner.CloneRepo:**

```csharp
public async Task<string> CloneRepo(string repoUrl, string sha)
{
    // 1. Create temp directory
    string temp = Path.Combine(
        Path.GetTempPath(),
        "refactai",
        Guid.NewGuid().ToString()
    );
    Directory.CreateDirectory(temp);

    // 2. Load GitHub token
    string token = GlobalTokenStore.Load();
    string repoToClone = GlobalRepoStore.Load();

    // 3. Embed token in URL for authentication
    if (!string.IsNullOrWhiteSpace(token))
    {
        repoToClone = repoToClone.Replace(
            "https://",
            $"https://{token}@"
        );
    }

    // 4. Execute git clone
    await Run("git",
        $"-c http.sslVerify=false clone --depth 1 {repoToClone} .",
        temp);

    // 5. Checkout specific commit if needed
    if (!string.IsNullOrWhiteSpace(sha) && sha != "HEAD")
    {
        await Run("git", $"fetch origin {sha} --depth 1", temp);
        await Run("git", $"checkout {sha}", temp);
    }

    return temp;
}
```

**What Happens:**
1. Creates a temporary directory (unique GUID)
2. Loads GitHub token from `secrets/github.token`
3. Modifies repo URL to include token: `https://TOKEN@github.com/...`
4. Runs `git clone --depth 1` (shallow clone, faster)
5. Checks out specific commit if SHA provided
6. Returns path to cloned repository

---

#### Step 6: Scan Files

```csharp
// Get all files in the repository
var files = await _runner.GetAllFiles(clonePath);
_logger.LogInformation($"[PrGrain] Found {files.Count} files");
```

**DotnetRunner.GetAllFiles:**

```csharp
public async Task<List<string>> GetAllFiles(string repoPath)
{
    var allFiles = Directory.GetFiles(repoPath, "*", SearchOption.AllDirectories);

    // Filter out .git directory and binary files
    return allFiles
        .Where(f => !f.Contains(".git"))
        .Where(f => IsTextFile(f))
        .ToList();
}
```

**What Happens:**
- Recursively scans all files in cloned repo
- Excludes `.git` directory
- Filters text files only (no binaries)
- Returns list of absolute file paths

---

#### Step 7: Refactor Each File

```csharp
// Process each file
foreach (var file in files)
{
    try
    {
        // 1. Detect language
        string ext = Path.GetExtension(file).ToLower();
        string lang = Detect(ext);  // .cs → "csharp", .js → "javascript"

        // 2. Skip unsupported files
        if (!Supports(lang))
        {
            _logger.LogDebug($"[PrGrain] Skipping unsupported file: {file}");
            continue;
        }

        _logger.LogInformation($"[PrGrain] Refactoring: {file} ({lang})");

        // 3. Read original code
        string code = await File.ReadAllTextAsync(file);

        // 4. AI Refactoring
        string updated = await _refactor.RefactorAsync(code, lang);

        // 5. Write back refactored code
        await File.WriteAllTextAsync(file, updated);

        _logger.LogInformation($"[PrGrain] ✓ Refactored: {file}");
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, $"[PrGrain] ✗ Error refactoring {file}");
        // Continue with next file (don't stop entire process)
    }
}
```

**Language Detection:**

```csharp
private string Detect(string ext) => ext switch
{
    ".cs" => "csharp",
    ".js" => "javascript",
    ".ts" => "typescript",
    ".java" => "java",
    ".py" => "python",
    ".c" => "c",
    ".cpp" or ".cc" => "cpp",
    ".go" => "go",
    ".rs" => "rust",
    ".rb" => "ruby",
    ".php" => "php",
    ".kt" => "kotlin",
    _ => "unknown"
};

private bool Supports(string lang) =>
    lang is "csharp" or "javascript" or "typescript" or
            "java" or "python" or "go" or "rust" or
            "c" or "cpp" or "ruby" or "php" or "kotlin";
```

**What Happens:**
1. For each file:
   - Detect programming language by extension
   - Skip if unsupported (markdown, images, etc.)
   - Read file contents
   - Call AI refactoring service
   - Overwrite file with refactored code
2. Errors in one file don't stop the whole process
3. Log success/failure for each file

---

#### Step 8: AI Refactoring

```
PrGrain → RefactorService.RefactorAsync(code, lang)
         → OllamaService.GenerateAsync(prompt)
```

**RefactorService.RefactorAsync:**

```csharp
public async Task<string> RefactorAsync(string code, string language)
{
    if (string.IsNullOrWhiteSpace(code))
        return code;

    // Build AI prompt
    string prompt = $@"
You are an AI code refactoring engine. Improve the following {language} code:

RULES:
- Clean formatting (proper indentation, spacing)
- Remove dead code (unused variables, imports)
- Simplify logic (use modern idioms)
- Improve naming (descriptive variable/function names)
- Keep behavior EXACTLY the same (no functional changes)

Return ONLY the refactored code, no explanations.

CODE:
{code}
";

    try
    {
        // Call Ollama AI
        string result = await _ollama.GenerateAsync(prompt);

        // Clean response (remove markdown code blocks)
        return CleanResponse(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "AI refactoring failed");
        return code;  // Fallback to original code
    }
}

private string CleanResponse(string raw)
{
    // Remove ```language and ``` markers if present
    if (raw.Contains("```"))
    {
        int start = raw.IndexOf("```");
        int end = raw.LastIndexOf("```");

        if (start >= 0 && end > start)
        {
            raw = raw.Substring(start + 3, end - start - 3);

            // Remove language identifier (```csharp)
            int newline = raw.IndexOf('\n');
            if (newline > 0)
                raw = raw.Substring(newline + 1);
        }
    }

    return raw.Trim();
}
```

**OllamaService.GenerateAsync:**

```csharp
public async Task<string> GenerateAsync(
    string prompt,
    string model = "deepseek-coder:6.7b")
{
    var psi = new ProcessStartInfo
    {
        FileName = "ollama",
        Arguments = $"run {model}",
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        StandardInputEncoding = Encoding.UTF8,
        StandardOutputEncoding = Encoding.UTF8
    };

    using var process = Process.Start(psi);

    // Send prompt to Ollama
    await process.StandardInput.WriteAsync(prompt);
    process.StandardInput.Close();

    // Read AI response
    string response = await process.StandardOutput.ReadToEndAsync();
    string errors = await process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
        throw new Exception($"Ollama failed: {errors}");

    return response.Trim();
}
```

**What Happens:**
1. RefactorService builds a detailed prompt
2. Prompt includes:
   - Instructions (clean code, remove dead code, etc.)
   - Original code
   - Language context
3. OllamaService spawns `ollama` CLI process
4. Feeds prompt to Ollama via stdin
5. Ollama runs DeepSeek Coder model (6.7B parameters)
6. AI generates refactored code
7. Response cleaned (removes markdown formatting)
8. Refactored code returned

**AI Model:**
- **DeepSeek Coder 6.7B** - Specialized for code generation
- Runs **locally** via Ollama (no cloud API calls)
- Takes 2-5 seconds per file (depends on hardware)

---

#### Step 9: Commit and Push

```
PrGrain → DotnetRunner.CommitAllAndPush(clonePath, branchName, commitMsg, baseBranch)
```

```csharp
// Create branch name
string prBranch = $"refact-ai-{DateTime.UtcNow:yyyyMMddHHmmss}";
// Example: refact-ai-20231215143022

string commitMessage = "Automated refactor by RefactAI";

_logger.LogInformation($"[PrGrain] Step 8: Commit and push to {prBranch}");

await _runner.CommitAllAndPush(clonePath, prBranch, commitMessage, "main");
```

**DotnetRunner.CommitAllAndPush:**

```csharp
public async Task CommitAllAndPush(
    string repoPath,
    string prBranchName,
    string commitMessage,
    string baseBranch = "main")
{
    string token = GlobalTokenStore.Load();

    // 1. Configure git identity
    await Run("git", "config user.name \"RefactAI Bot\"", repoPath);
    await Run("git", "config user.email \"bot@refactai.local\"", repoPath);

    // 2. Checkout base branch and pull latest
    await Run("git", $"checkout {baseBranch}", repoPath);
    await Run("git", $"pull origin {baseBranch}", repoPath);

    // 3. Create new PR branch
    await Run("git", $"checkout -b {prBranchName}", repoPath);

    // 4. Stage all changes
    await Run("git", "add .", repoPath);

    // 5. Check if there are changes
    string status = await RunAndCapture("git", "status --porcelain", repoPath);
    if (string.IsNullOrWhiteSpace(status))
    {
        Console.WriteLine("[DotnetRunner] No changes to commit.");
        return;
    }

    // 6. Commit
    await Run("git", $"commit -m \"{commitMessage}\"", repoPath);

    // 7. Get current remote URL
    string remoteUrl = await RunAndCapture("git", "remote get-url origin", repoPath);

    // Parse owner/repo from URL
    // Example: https://github.com/owner/repo.git
    var match = Regex.Match(remoteUrl, @"github\.com[/:](.*?)/(.*?)(\.git)?$");
    string owner = match.Groups[1].Value;
    string repo = match.Groups[2].Value;

    // 8. Update remote URL with token
    string authenticatedUrl =
        $"https://{token}:x-oauth-basic@github.com/{owner}/{repo}.git";
    await Run("git", $"remote set-url origin {authenticatedUrl}", repoPath);

    // 9. Push branch to GitHub
    await Run("git", $"push -u origin {prBranchName}", repoPath);

    _logger.LogInformation($"[DotnetRunner] Pushed {prBranchName} to GitHub");
}
```

**What Happens:**
1. Configure git user (RefactAI Bot)
2. Checkout base branch (usually `main`)
3. Create new branch (`refact-ai-TIMESTAMP`)
4. Stage all modified files (`git add .`)
5. Check if there are actual changes
6. Commit with message
7. Update remote URL to include GitHub token
8. Push branch to GitHub
9. Branch now exists on GitHub, ready for PR

---

#### Step 10: Create Pull Request

```
PrGrain → GitHubService.CreatePullRequest(repoUrl, headBranch, baseBranch, title, body)
```

```csharp
string prTitle = $"RefactorAI automated refactor ({prBranch})";
string prBody = $@"
## Automated Refactoring by RefactAI

This PR contains automated code improvements generated at {DateTime.UtcNow:O}.

### Changes:
- Clean code formatting
- Dead code removal
- Logic simplification
- Improved naming conventions

**⚠️ Please review carefully before merging.**
";

_logger.LogInformation("[PrGrain] Step 9: Create GitHub PR");

string prUrl = await _gh.CreatePullRequest(
    request.RepoUrl,
    prBranch,     // head: refact-ai-20231215143022
    "main",       // base: main
    prTitle,
    prBody
);

_logger.LogInformation($"[PrGrain] ✓ PR created: {prUrl}");
```

**GitHubService.CreatePullRequest:**

```csharp
public async Task<string> CreatePullRequest(
    string repoUrl,
    string headBranch,
    string baseBranch,
    string title,
    string body)
{
    // Parse owner/repo from URL
    var (owner, repo) = ParseOwnerRepo(repoUrl);
    // Example: ("octocat", "hello-world")

    // GitHub API endpoint
    var endpoint = $"https://api.github.com/repos/{owner}/{repo}/pulls";

    // Create payload
    var payload = new
    {
        title,
        head = headBranch,
        @base = baseBranch,
        body
    };

    // Send POST request
    var resp = await _http.PostAsJsonAsync(endpoint, payload);
    var text = await resp.Content.ReadAsStringAsync();

    if (!resp.IsSuccessStatusCode)
        throw new Exception($"Failed to create PR: {resp.StatusCode} {text}");

    // Extract PR URL from response
    using var doc = JsonDocument.Parse(text);
    if (doc.RootElement.TryGetProperty("html_url", out var u))
        return u.GetString() ?? string.Empty;

    return text;
}
```

**GitHubService Constructor (Authentication):**

```csharp
public GitHubService(HttpClient http)
{
    _http = http;
    _http.DefaultRequestHeaders.UserAgent.ParseAdd("RefactAI-Bot");

    // Load GitHub token
    string? token = GlobalTokenStore.Load();
    if (string.IsNullOrWhiteSpace(token))
        throw new InvalidOperationException("GitHub token not set");

    // Add Authorization header
    _http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("token", token);
}
```

**What Happens:**
1. Parse repository owner and name from URL
2. Build GitHub API endpoint
3. Create JSON payload with PR details
4. Send authenticated POST request to GitHub API
5. GitHub creates the pull request
6. Extract PR URL from response
7. Return URL for logging

**GitHub API Response Example:**

```json
{
  "id": 123456,
  "number": 42,
  "state": "open",
  "title": "RefactorAI automated refactor (refact-ai-20231215143022)",
  "html_url": "https://github.com/owner/repo/pull/42",
  "user": { "login": "refactai-bot" },
  "head": { "ref": "refact-ai-20231215143022" },
  "base": { "ref": "main" }
}
```

---

#### Step 11: Save Local Copy (Optional)

```csharp
// Save refactored code locally for debugging
var output = Path.Combine(
    "/Users/suraj/RefactAI_Output/",
    ExtractRepoName(request.RepoUrl),  // "myrepo"
    request.Sha                         // "HEAD" or commit SHA
);

CopyDirectory(clonePath, output);
_logger.LogInformation($"[PrGrain] Saved local copy to: {output}");
```

**What Happens:**
- Copies refactored repository to permanent location
- Useful for debugging, comparison, or backup
- Structure: `/RefactAI_Output/<repo-name>/<sha>/`

---

#### Step 12: Cleanup

```csharp
// Delete temporary clone directory
Directory.Delete(clonePath, recursive: true);

_logger.LogInformation("[PrGrain] ✓ Processing complete!");
```

**What Happens:**
- Removes temporary clone directory
- Frees disk space
- Process complete!

---

## Component Deep Dive

### 1. Orleans Grains

#### RepoGrain

**Purpose:** Lightweight router/gateway

```csharp
public interface IRepoGrain : IGrainWithStringKey
{
    Task EnqueueWork(RepoWorkItem item);
}

// Grain key: Repository URL
// Example: "https://github.com/owner/repo"
```

**Responsibilities:**
- Receive work from API
- Validate and normalize input
- Route to appropriate PrGrain
- Act as a stable interface

**Why it exists:**
- Separation of concerns
- API talks to RepoGrain (stable interface)
- PrGrain can change implementation without affecting API

---

#### PrGrain

**Purpose:** Heavy worker for processing

```csharp
public interface IPrGrain : IGrainWithStringKey
{
    Task<PrResult> ProcessPr(PrRequest request);
}

// Grain key: Repository URL (same as RepoGrain)
// Example: "https://github.com/owner/repo"
```

**Responsibilities:**
- Clone repository
- Scan files
- Refactor each file with AI
- Commit changes
- Push to GitHub
- Create pull request
- Handle errors gracefully

**State Management:**
- Each PrGrain processes one repository at a time
- Orleans ensures serialized execution (no race conditions)
- Long-running operations use timers (non-blocking)

---

### 2. Services

#### DotnetRunner

**File:** RefactAI.Common/Runner/DotnetRunner.cs

**Purpose:** Execute git commands

**Key Methods:**

| Method | Purpose |
|--------|---------|
| `CloneRepo` | Clone repository to temp directory |
| `GetAllFiles` | List all files in repository |
| `CommitAllAndPush` | Stage, commit, and push changes |
| `Run` | Execute shell command |
| `RunAndCapture` | Execute and capture output |

**Example:**

```csharp
// Clone
var path = await runner.CloneRepo("https://github.com/owner/repo", "HEAD");

// Commit
await runner.CommitAllAndPush(path, "my-branch", "My commit", "main");
```

---

#### RefactorService

**File:** RefactAI.Refactor/RefactorService.cs

**Purpose:** Orchestrate AI refactoring

**Key Methods:**

| Method | Purpose |
|--------|---------|
| `RefactorAsync` | Refactor code using AI |
| `CleanResponse` | Remove markdown formatting |

**Prompt Engineering:**

```csharp
string prompt = $@"
You are an AI code refactoring engine. Improve the following {language} code:

RULES:
- Clean formatting
- Remove dead code
- Simplify logic
- Improve naming
- Keep behavior the same

Return ONLY the refactored code.

CODE:
{code}
";
```

---

#### OllamaService

**File:** RefactAI.Common/AI/OllamaService.cs

**Purpose:** Interface with Ollama AI

**How it works:**

```csharp
// 1. Spawn Ollama process
var process = Process.Start("ollama", "run deepseek-coder:6.7b");

// 2. Send prompt via stdin
await process.StandardInput.WriteAsync(prompt);
process.StandardInput.Close();

// 3. Read response from stdout
string response = await process.StandardOutput.ReadToEndAsync();

// 4. Wait for completion
await process.WaitForExitAsync();
```

**Model:** DeepSeek Coder 6.7B
- Specialized for code generation
- 6.7 billion parameters
- Runs locally (no API calls)
- Requires Ollama installed

---

#### GitHubService

**File:** RefactAI.Orleans.Grains/GithubService.cs

**Purpose:** Interact with GitHub API

**Key Methods:**

| Method | Purpose |
|--------|---------|
| `CreatePullRequest` | Create PR via API |
| `ParseOwnerRepo` | Extract owner/repo from URL |

**Authentication:**

```csharp
// Add Authorization header with GitHub token
_http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("token", token);

// All requests now authenticated
await _http.PostAsJsonAsync(endpoint, payload);
```

**GitHub API Endpoints:**

| Endpoint | Purpose |
|----------|---------|
| `POST /repos/{owner}/{repo}/pulls` | Create pull request |
| `GET  /repos/{owner}/{repo}` | Get repo info |

---

### 3. Configuration

#### GlobalTokenStore

**Purpose:** Store GitHub token persistently

```csharp
public static class GlobalTokenStore
{
    private static readonly string TokenPath =
        "/Users/suraj/.../RefactAI/secrets/github.token";

    public static void Save(string token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(TokenPath)!);
        File.WriteAllText(TokenPath, token);
    }

    public static string Load()
    {
        if (!File.Exists(TokenPath))
            throw new Exception("Token file not found");

        return File.ReadAllText(TokenPath).Trim();
    }
}
```

**Usage:**

```csharp
// Save token (from API)
GlobalTokenStore.Save("ghp_xxxxxxxxxxxx");

// Load token (in services)
string token = GlobalTokenStore.Load();
```

---

#### GlobalRepoStore

**Purpose:** Store repository URL

```csharp
public static class GlobalRepoStore
{
    private static readonly string RepoPath =
        "/Users/suraj/.../RefactAI/secrets/Repo.url";

    public static void Save(string repoUrl) { /* ... */ }
    public static string Load() { /* ... */ }
}
```

**Why separate files?**
- Simple persistence (no database needed)
- Easy to edit manually
- Local-first security model
- Configuration lives with code

---

## Execution Timeline

### ⏱️ Typical Execution Times

**Small Repository (10 files):**

| Phase | Duration | Details |
|-------|----------|---------|
| API → RepoGrain | 10 ms | HTTP request + grain activation |
| RepoGrain → PrGrain | 5 ms | Grain-to-grain call |
| Clone repository | 2-5 sec | Git clone (shallow) |
| Scan files | 100 ms | File system traversal |
| Refactor files (10) | 20-50 sec | 2-5 sec per file (AI inference) |
| Commit & push | 2-3 sec | Git operations + network |
| Create PR | 500 ms | GitHub API call |
| **Total** | **25-60 sec** | |

**Medium Repository (100 files):**

| Phase | Duration | Details |
|-------|----------|---------|
| API → PrGrain | 15 ms | |
| Clone repository | 5-10 sec | Larger repo |
| Scan files | 500 ms | More files |
| Refactor files (100) | 3-8 min | 2-5 sec × 100 |
| Commit & push | 5 sec | More data to push |
| Create PR | 500 ms | |
| **Total** | **3-10 min** | |

**Large Repository (1000 files):**

| Phase | Duration | Details |
|-------|----------|---------|
| API → PrGrain | 20 ms | |
| Clone repository | 20-30 sec | Large repo |
| Scan files | 2 sec | Many files |
| Refactor files (1000) | 30-80 min | 2-5 sec × 1000 |
| Commit & push | 10-20 sec | Large changeset |
| Create PR | 500 ms | |
| **Total** | **30-90 min** | |

---

### 🚀 Performance Optimizations

#### Current Optimizations:

1. **Shallow Clone** (`--depth 1`)
   - Only downloads latest commit
   - Saves time and disk space

2. **Parallel File Processing** (Future)
   - Currently sequential
   - Could process multiple files concurrently

3. **Skip Unsupported Files**
   - No time wasted on non-code files
   - Early exit for binaries, images, etc.

4. **Local AI (No Network)**
   - No API latency
   - No rate limits
   - Faster than cloud APIs

#### Future Optimizations:

1. **Distributed Processing**
   - Run multiple PrGrains on different machines
   - Orleans handles distribution automatically

2. **Batch Refactoring**
   - Process multiple files in single AI call
   - Reduces overhead

3. **Incremental Processing**
   - Only refactor changed files
   - Cache previous refactorings

4. **GPU Acceleration**
   - Use GPU for faster AI inference
   - Ollama supports CUDA

---

## Code Examples

### Example 1: Triggering Refactoring from Code

```csharp
using Orleans;
using RefactAI.Orleans.Contracts;

// Connect to Orleans cluster
var client = new ClientBuilder()
    .UseStaticClustering(options =>
    {
        options.Gateways.Add(new Uri("gwy.tcp://127.0.0.1:30000"));
    })
    .Build();

await client.Connect();

// Trigger refactoring
var grain = client.GetGrain<IRepoGrain>("https://github.com/owner/repo");
await grain.EnqueueWork(new RepoWorkItem(
    RepoUrl: "https://github.com/owner/repo",
    Branch: "main",
    Sha: "HEAD",
    Kind: "Manual"
));

Console.WriteLine("Refactoring started!");
```

---

### Example 2: Manual File Refactoring

```csharp
using RefactAI.Refactor;
using RefactAI.Common.AI;

// Initialize services
var ollama = new OllamaService();
var refactor = new RefactorService(ollama, logger);

// Read code
string code = await File.ReadAllTextAsync("MyFile.cs");

// Refactor
string refactored = await refactor.RefactorAsync(code, "csharp");

// Save
await File.WriteAllTextAsync("MyFile.cs", refactored);

Console.WriteLine("File refactored!");
```

---

### Example 3: Create PR Directly

```csharp
using RefactAI.Orleans.Grains;

// Initialize service
var http = new HttpClient();
var github = new GitHubService(http);

// Create PR
string prUrl = await github.CreatePullRequest(
    repoUrl: "https://github.com/owner/repo",
    headBranch: "my-feature-branch",
    baseBranch: "main",
    title: "My awesome feature",
    body: "This PR adds X, Y, and Z"
);

Console.WriteLine($"PR created: {prUrl}");
```

---

## Summary

### 🎯 What is Orleans Silo?

The **Orleans Silo** is:
- The **backend processing engine** that hosts Grains
- A **distributed actor system** for scalable processing
- Runs on **port 11111** (silo-to-silo) and **port 30000** (gateway)
- Registers all **services** (Git, AI, GitHub) that Grains need
- Ensures **only one instance** of each Grain exists per key

### 🔄 Full Flow Summary

1. **User** configures GitHub token via API
2. **User** triggers refactoring (manual or webhook)
3. **API** receives request, gets `RepoGrain` from Orleans
4. **RepoGrain** validates, normalizes, forwards to `PrGrain`
5. **PrGrain** clones repository using `DotnetRunner`
6. **PrGrain** scans all files
7. **For each file:**
   - Detect language
   - Call `RefactorService`
   - `RefactorService` calls `OllamaService`
   - AI generates refactored code
   - Write back to file
8. **PrGrain** commits and pushes changes via `DotnetRunner`
9. **PrGrain** creates PR via `GitHubService`
10. **PrGrain** saves local copy (optional)
11. **Done!** PR ready for human review on GitHub

### 🌟 Key Takeaways

- **Orleans Silo** = Backend processing engine
- **Grains** = Isolated, stateful actors (one per repo)
- **RepoGrain** = Router, **PrGrain** = Worker
- **Services** = Git, AI, GitHub operations
- **Flow** = API → Grain → Clone → Refactor → Commit → PR
- **Local AI** = Fast, private, no API costs
- **Human-in-the-loop** = PR review before merge

---

**Your RefactAI system is a sophisticated distributed application that leverages Orleans for scalability, Ollama for local AI, and GitHub for version control—all orchestrated through a clean, modular architecture!** 🚀
