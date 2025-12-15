using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RefactAI.Common;

namespace RefactAI.Common.Runner
{
    public class DotnetRunner : IDotnetRunner
    {
        private const string FixedPath = "/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin";

        public async Task<string> CloneRepo(string repoUrl, string sha)
        {
            Console.WriteLine("ENV GITHUB_TOKEN clone = " + GlobalTokenStore.Load());
            Console.WriteLine("ENV REPO_URL clone = " + GlobalRepoStore.Load());

            string temp = Path.Combine(Path.GetTempPath(), "refactai", Guid.NewGuid().ToString());
            Directory.CreateDirectory(temp);

            // Make sure child processes can find git
            Environment.SetEnvironmentVariable("PATH", FixedPath);

            // Try to use token from env if present (do NOT log the token)
            string token = GlobalTokenStore.Load();
            string repoToClone = GlobalRepoStore.Load();//"https://github.com/SurajGangwani0711/Chec-RefactAI";
            Console.WriteLine("TOken is"+token);
            Console.WriteLine("Repo is"+repoToClone);
            if (!string.IsNullOrWhiteSpace(token))
            {
                // embed token in URL for clone/push operations
                // example: https://{token}@github.com/owner/repo.git
                if (repoToClone.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    repoToClone = repoToClone.Replace("https://", $"https://{token}@");
            }

            try
            {
                await Run("git", $"-c http.sslVerify=false clone --depth 1 {repoToClone} .", temp);

                if (!string.IsNullOrWhiteSpace(sha) && sha != "HEAD")
                {
                    // fetch and checkout the given sha
                    await Run("git", $"-c http.sslVerify=false fetch origin {sha} --depth 1", temp);
                    await Run("git", $"checkout {sha}", temp);
                }

                Console.WriteLine($"✅ Clone success: {temp}");
                return temp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Clone failed: {ex.Message}\n{ex}");
                throw;
            }
        }

        public List<string> GetAllSourceFiles(string repoPath)
        {
            string[] patterns = new[]
            {
                "*.cs", "*.js", "*.ts", "*.jsx", "*.tsx",
                "*.java", "*.kt", "*.go", "*.rs",
                "*.py", "*.rb", "*.php",
                "*.cpp", "*.c", "*.h", "*.hpp",
            };

            var files = new List<string>();
            foreach (var pattern in patterns)
                files.AddRange(Directory.GetFiles(repoPath, pattern, SearchOption.AllDirectories));

            return files;
        }

        public string DetectLanguage(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".cs" => "csharp",
                ".js" => "javascript",
                ".ts" => "typescript",
                ".jsx" => "javascript",
                ".tsx" => "typescript",
                ".java" => "java",
                ".kt" => "kotlin",
                ".go" => "go",
                ".rs" => "rust",
                ".py" => "python",
                ".rb" => "ruby",
                ".php" => "php",
                ".cpp" => "cpp",
                ".c" => "c",
                ".h" => "c-header",
                _ => "unknown"
            };
        }

        private async Task Run(string file, string args, string workingDir)
        {
            var psi = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            // Ensure git is found
            psi.EnvironmentVariables["PATH"] = FixedPath;

            using var proc = Process.Start(psi)!;
            string stdout = await proc.StandardOutput.ReadToEndAsync();
            string stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            Console.WriteLine("GIT STDOUT: " + stdout);
            if (!string.IsNullOrWhiteSpace(stderr))
                Console.WriteLine("GIT STDERR: " + stderr);

            if (proc.ExitCode != 0)
                throw new Exception($"Git command failed: {file} {args}\n\nSTDOUT:{stdout}\n\nSTDERR:{stderr}");
        }

        public async Task<string> RunAndCapture(string file, string args, string workingDir)
        {
            var psi = new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            psi.EnvironmentVariables["PATH"] = FixedPath;

            using var proc = Process.Start(psi)!;
            string output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();
            return output;
        }

        public Task<List<string>> GetAllFiles(string repoPath)
        {
            var files = Directory.GetFiles(repoPath, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + ".git" + Path.DirectorySeparatorChar))
                .ToList();

            return Task.FromResult(files);
        }

        // ---------------------------------------------------------------------
        // New: commit all changes, create branch and push using token
        // ---------------------------------------------------------------------
        public async Task CommitAllAndPush(
            string repoPath,
            string prBranchName,
            string commitMessage,
            string baseBranch="main")
        {
            // -------------------------------------------------------
            // 0. Ensure token exists
            // -------------------------------------------------------
            string token = GlobalTokenStore.Load();

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("GITHUB_TOKEN not set in environment variables!");

            // -------------------------------------------------------
            // 1. Configure git identity
            // -------------------------------------------------------
            await Run("git", "config user.name \"RefactAI Bot\"", repoPath);
            await Run("git", "config user.email \"bot@refactai.local\"", repoPath);

            // -------------------------------------------------------
            // 2. Ensure we're on latest base branch
            // -------------------------------------------------------
            await Run("git", $"checkout {baseBranch}", repoPath);
            await Run("git", $"pull origin {baseBranch}", repoPath);

            // -------------------------------------------------------
            // 3. Create new branch
            // -------------------------------------------------------
            await Run("git", $"checkout -b {prBranchName}", repoPath);

            // -------------------------------------------------------
            // 4. Add & commit changes
            // -------------------------------------------------------
            await Run("git", "add .", repoPath);

            // If no changes – exit gracefully
            string status = await RunAndCapture("git", "status --porcelain", repoPath);
            if (string.IsNullOrWhiteSpace(status))
            {
                Console.WriteLine("No changes to commit.");
                return;
            }

            await Run("git", $"commit -m \"{commitMessage}\"", repoPath);

            // -------------------------------------------------------
            // 5. Fix remote URL (this is the important part)
            // -------------------------------------------------------
            // We always enforce: https://TOKEN:x-oauth-basic@github.com/user/repo.git
            string remoteUrl = await RunAndCapture("git", "remote get-url origin", repoPath);

            // Normalize: remove newline
            remoteUrl = remoteUrl.Trim();

            // Must end in .git or GitHub rejects it
            if (!remoteUrl.EndsWith(".git"))
                remoteUrl += ".git";

            // Extract owner + repo
            var match = Regex.Match(remoteUrl, @"github\.com[/:](.*?)/(.*?)(\.git)?$");
            if (!match.Success)
                throw new Exception($"Unable to parse GitHub repo URL: {remoteUrl}");

            string owner = match.Groups[1].Value;
            string repo = match.Groups[2].Value;

            string authenticatedUrl =
                $"https://{token}:x-oauth-basic@github.com/{owner}/{repo}.git";

            // Apply new remote URL
            await Run("git", $"remote set-url origin {authenticatedUrl}", repoPath);

            // -------------------------------------------------------
            // 6. Push new branch
            // -------------------------------------------------------
            await Run("git", $"push -u origin {prBranchName}", repoPath);
        }

    }
}
