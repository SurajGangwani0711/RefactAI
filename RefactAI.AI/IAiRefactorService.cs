using System.Threading.Tasks;

namespace RefactAI.AI
{
    public interface IAiRefactorService
    {
        Task<string> RefactorCodeAsync(string code);
    }
}
