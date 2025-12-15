using System;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using RefactAI.Common.AI;


namespace RefactAI.Refactor
{
    public class RefactorService : IRefactorService
    {
        private readonly ILogger<RefactorService> _logger;
        private readonly IOllamaService _ollama;

        public RefactorService(ILogger<RefactorService> logger, IOllamaService ollama)
        {
            _logger = logger;
            _ollama = ollama;
        }

        public async Task<string> RefactorAsync(string code, string language)
        {
            if (string.IsNullOrWhiteSpace(code))
                return code;

            string prompt = $@"
You are an AI code refactoring engine. Improve the following {language} code:

- clean formatting
- remove dead code
- simplify logic
- improve naming
- keep behavior EXACTLY the same

Return ONLY the refactored code.

CODE:
{code}
";

            try
            {
                string result = await _ollama.GenerateAsync(prompt);
                return CleanResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI refactoring failed. Returning original code.");
                return code;
            }
        }

        private string CleanResponse(string raw)
        {
            // Remove ``` blocks if the model includes them
            if (raw.Contains("```"))
            {
                int start = raw.IndexOf("```");
                int end = raw.LastIndexOf("```");

                if (start >= 0 && end > start)
                {
                    raw = raw[(start + 3)..end];
                }
            }

            return raw.Trim();
        }
    }
}
