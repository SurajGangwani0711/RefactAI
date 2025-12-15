using System.Text.Json;

namespace RefactAI.Common
{
    public static class TokenLoader
    {
       public static string GetTokenPath()
        {
            var solutionRoot =
                Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;

            var tokenPath = Path.Combine(
                solutionRoot,
                "RefactAI.Api",
                "config",
                "Token.json"
            );

            return tokenPath;
        }

        public static string GetToken()
        {
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            Console.WriteLine("GIT TOken is", token);
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("❌ GITHUB_TOKEN not set. Use the React UI to set it.");

            return token;
        }

        public static void SaveToken(string token)
        {
            var obj = new TokenFile { github_token = token };
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetTokenPath(), json);
        }

        private class TokenFile
        {
            public string? github_token { get; set; }
        }
    }
}
