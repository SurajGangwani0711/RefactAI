using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace RefactAI.Orleans.Grains.Utils
{
    public static class DotnetRunner
    {
        public static async Task<BuildTestResult> RunAsync(string workDir)
        {
            // 1️⃣ Restore dependencies
            var restore = await Proc.Run("dotnet", "restore", workDir);
            if (restore.ExitCode != 0)
                return BuildTestResult.Fail("RESTORE", restore.StdErr);

            // 2️⃣ Build the project
            var build = await Proc.Run("dotnet", "build -c Release", workDir);
            if (build.ExitCode != 0)
                return BuildTestResult.Fail("BUILD", build.StdErr);

            // 3️⃣ Run unit tests
            var test = await Proc.Run(
                "dotnet",
                "test -c Release --logger \"trx;LogFileName=test.trx\"",
                workDir
            );

            return new BuildTestResult(
                true,
                build.StdOut,
                test.StdOut,
                test.ExitCode == 0
            );
        }
    }

    // Represents the result of a build/test cycle
    public record BuildTestResult(
        bool Success,
        string BuildLogs,
        string TestLogs,
        bool TestsPassed
    )
    {
        public static BuildTestResult Fail(string stage, string message)
            => new(false, $"{stage} FAILED:\n{message}", "", false);

        public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    // Generic helper for running external processes
    public static class Proc
    {
        public static async Task<(int ExitCode, string StdOut, string StdErr)> Run(
            string fileName, string args, string workDir)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                WorkingDirectory = workDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi)!;
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            proc.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
            proc.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            await proc.WaitForExitAsync();

            return (proc.ExitCode, stdout.ToString(), stderr.ToString());
        }
    }
}
