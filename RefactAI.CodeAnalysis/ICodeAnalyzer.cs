using System.Collections.Generic;
using System.Threading.Tasks;

namespace RefactAI.CodeAnalysis
{
    public interface ICodeAnalyzer
    {
        Task<string> Analyze(IEnumerable<string> updatedFiles);
    }
}
