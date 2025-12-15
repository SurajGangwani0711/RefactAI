using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RefactAI.RefactorAgent
{
    public static class RefactorAgent
    {
        public static async Task RunAsync(string solutionPath)
        {
             MSBuildLocator.RegisterDefaults();
            using var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionPath);

            foreach (var project in solution.Projects)
            {
                Console.WriteLine($"🔍 Scanning project: {project.Name}");
                foreach (var doc in project.Documents)
                {
                    if (!doc.Name.EndsWith(".cs")) continue;

                    var newDoc = await NullGuardRefactor.ApplyAsync(doc, CancellationToken.None);

                    if (newDoc != doc)
                    {
                        var oldText = await doc.GetTextAsync();
                        var newText = await newDoc.GetTextAsync();

                        if (!oldText.ContentEquals(newText))
                        {
                            // Generate the diff text
                            var diffText = Diff(oldText.ToString(), newText.ToString());

                            // Show preview in console
                            Console.WriteLine($"\n📄 Proposed change for: {doc.Name}");
                            Console.WriteLine(new string('-', 80));
                            Console.WriteLine(diffText);
                            Console.WriteLine(new string('-', 80));

                            // Ask for confirmation
                            Console.Write("💡 Apply this change to the original file? (y/n): ");
                            var response = Console.ReadLine()?.Trim().ToLowerInvariant();

                            if (response == "y")
                            {
                                // Get the physical path of the source file
                                var filePath = doc.FilePath;
                                if (filePath != null)
                                {
                                    // Write the updated content back to the original file
                                    var newTextString = await newDoc.GetTextAsync();
                                    File.WriteAllText(filePath, newTextString.ToString());
                                    Console.WriteLine($"✅ Changes applied to {filePath}\n");
                                }
                                else
                                {
                                    Console.WriteLine("⚠️ Unable to determine file path for this document.");
                                }
                            }
                            else
                            {
                                // If rejected, save patch to /tmp/patches for later review
                                var patchDir = "/tmp/patches";
                                Directory.CreateDirectory(patchDir);
                                var patchFile = Path.Combine(patchDir, doc.Name + ".diff");
                                File.WriteAllText(patchFile, diffText);
                                Console.WriteLine($"💾 Skipped — patch saved to {patchFile}\n");
                            }
                        }
                    }
                }
            }
        }

        private static string Diff(string oldText, string newText)
        {
            // Simple inline diff — can later replace with DiffPlex library
            return $"--- Original\n+++ Refactored\n{newText}";
        }
    }
}
