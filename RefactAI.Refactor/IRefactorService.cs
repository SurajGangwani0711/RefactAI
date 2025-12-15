using System.Threading.Tasks;

namespace RefactAI.Refactor
{
    public interface IRefactorService
    {
        /// <summary>
        /// Refactors the given code file using the appropriate engine.
        /// </summary>
        /// <param name="code">The contents of the file</param>
        /// <param name="language">Detected language ('csharp', 'js', 'java', etc.)</param>
        /// <returns>Refactored code (or the original if unchanged)</returns>
        Task<string> RefactorAsync(string code, string language);
    }
}
