using System;
using System.Threading.Tasks;

namespace RefactAI.RefactorAgent
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run --project RefactAI.RefactorAgent -- <path-to-solution>");
                return;
            }

            var solutionPath = args[0];
            Console.WriteLine($"🚀 Starting RefactorAgent on solution: {solutionPath}");

            await RefactorAgent.RunAsync(solutionPath);

            Console.WriteLine("✅ Refactor process complete.");
        }
    }
}
