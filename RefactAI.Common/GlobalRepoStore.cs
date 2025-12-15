using System;
using System.IO;

namespace RefactAI.Common
{
    public static class GlobalRepoStore
    {
        private static readonly string RepoPath =
            "/Users/suraj/Documents/Personal_project/RefactAI/secrets/Repo.url";

        public static void Save(string repoUrl)
        {
            if (string.IsNullOrWhiteSpace(repoUrl))
                throw new Exception("Repo URL cannot be empty");

            Directory.CreateDirectory(Path.GetDirectoryName(RepoPath)!);
            File.WriteAllText(RepoPath, repoUrl.Trim());
        }

        public static string Load()
        {
            if (!File.Exists(RepoPath))
                throw new Exception($"Repo URL file not found at: {RepoPath}");

            var repo = File.ReadAllText(RepoPath).Trim();

            if (string.IsNullOrWhiteSpace(repo))
                throw new Exception("Repo URL file is empty");

            return repo;
        }
    }
}
