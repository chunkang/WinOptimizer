namespace WinOptimizer.Services;

using System.Diagnostics;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class UninstallService
{
    public async Task<(int succeeded, int failed, List<string> errors)> UninstallSelected(
        IEnumerable<DetectedSoftware> software,
        IProgress<string>? progress = null)
    {
        var succeeded = 0;
        var failed = 0;
        var errors = new List<string>();

        foreach (var item in software)
        {
            progress?.Report($"Uninstalling {item.DisplayName}...");
            LogHelper.Log($"Uninstalling: {item.DisplayName}");

            var uninstallCommand = item.QuietUninstallString ?? item.UninstallString;
            if (string.IsNullOrWhiteSpace(uninstallCommand))
            {
                errors.Add($"{item.DisplayName}: No uninstall command found");
                failed++;
                continue;
            }

            try
            {
                var exitCode = await RunUninstallCommand(uninstallCommand);
                if (exitCode == 0)
                {
                    LogHelper.Log($"Successfully uninstalled: {item.DisplayName}");
                    succeeded++;
                }
                else
                {
                    var msg = $"{item.DisplayName}: Uninstall exited with code {exitCode}";
                    LogHelper.Log(msg);
                    errors.Add(msg);
                    failed++;
                }
            }
            catch (Exception ex)
            {
                var msg = $"{item.DisplayName}: {ex.Message}";
                LogHelper.Log($"Uninstall error: {msg}");
                errors.Add(msg);
                failed++;
            }
        }

        return (succeeded, failed, errors);
    }

    private static async Task<int> RunUninstallCommand(string command)
    {
        // Parse command - some uninstall strings are wrapped in quotes
        string fileName;
        string arguments;

        if (command.StartsWith('"'))
        {
            var endQuote = command.IndexOf('"', 1);
            if (endQuote > 0)
            {
                fileName = command[1..endQuote];
                arguments = command[(endQuote + 1)..].TrimStart();
            }
            else
            {
                fileName = command;
                arguments = string.Empty;
            }
        }
        else
        {
            var spaceIndex = command.IndexOf(' ');
            if (spaceIndex > 0)
            {
                fileName = command[..spaceIndex];
                arguments = command[(spaceIndex + 1)..];
            }
            else
            {
                fileName = command;
                arguments = string.Empty;
            }
        }

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
        };

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start uninstall process");

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
