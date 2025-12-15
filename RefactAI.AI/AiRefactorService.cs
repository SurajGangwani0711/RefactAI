using System.Threading.Tasks;

namespace RefactAI.AI
{
    public class AiRefactorService : IAiRefactorService
    {
        public Task<string> RefactorCodeAsync(string code)
        {
            // placeholder — later we will call OpenAI / Azure
            return Task.FromResult("// AI Refactor (stub)\n" + code);
        }
    }
}
