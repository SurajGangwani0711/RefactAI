using System.Threading.Tasks;

namespace RefactAI.Common.AI
{
    public interface IOllamaService
    {
        Task<string> GenerateAsync(string prompt, string model = "deepseek-coder:6.7b");
    }
}
