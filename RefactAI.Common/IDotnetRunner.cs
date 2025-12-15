using System.Collections.Generic;
using System.Threading.Tasks;

namespace RefactAI.Common.Runner
{
    public interface IDotnetRunner
    {
        Task<string> CloneRepo(string repoUrl, string sha);
        List<string> GetAllSourceFiles(string repoPath);
        string DetectLanguage(string filePath);
        Task<string> RunAndCapture(string file, string args, string workingDir);
        Task<List<string>> GetAllFiles(string repoPath);

        // New: commit all, create branch and push (uses token from env)
        Task CommitAllAndPush(string repoPath, string prBranchName, string commitMessage, string baseBranch = "main");
    }
}
