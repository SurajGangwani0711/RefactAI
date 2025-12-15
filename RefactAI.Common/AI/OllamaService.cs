using RefactAI.Common.AI;
using System.Diagnostics;
using System.Text;

namespace RefactAI.Orleans.Grains
{
    public class OllamaService : IOllamaService
    {
        public async Task<string> GenerateAsync(string prompt, string model = "deepseek-coder:6.7b")
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ollama",
                Arguments = $"run {model}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);

            await process.StandardInput.WriteAsync(prompt);
            process.StandardInput.Close();

            string response = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return response.Trim();
        }
    }
}
