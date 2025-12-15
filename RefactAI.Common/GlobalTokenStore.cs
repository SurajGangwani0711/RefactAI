using System;
using System.IO;

namespace RefactAI.Common
{
    public static class GlobalTokenStore
    {
        private static readonly string TokenPath =
            "/Users/suraj/Documents/Personal_project/RefactAI/secrets/github.token";

        public static void Save(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token cannot be empty");

            Directory.CreateDirectory(Path.GetDirectoryName(TokenPath)!);
            File.WriteAllText(TokenPath, token);
        }

        public static string Load()
        {
            if (!File.Exists(TokenPath))
                throw new Exception($"Global token file not found at: {TokenPath}");

            var token = File.ReadAllText(TokenPath).Trim();

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Global token file is empty");

            return token;
        }
    }
}
