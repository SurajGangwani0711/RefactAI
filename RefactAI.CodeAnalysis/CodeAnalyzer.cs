using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefactAI.CodeAnalysis
{
    public class CodeAnalyzer : ICodeAnalyzer
    {
        public Task<string> Analyze(IEnumerable<string> updatedFiles)
        {
            var files = updatedFiles.ToList();
            int lines = files.Sum(f => f?.Split('\n').Length ?? 0);
            var result = $"Files: {files.Count}\nTotal lines: {lines}\nIssues: 0 (stub)";
            return Task.FromResult(result);
        }
    }
}
