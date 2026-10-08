// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-10-07)
// ============================================================================

namespace WinOptimizer.Services;

using System.Diagnostics;
using System.Text.RegularExpressions;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class UninstallService
{
    private static readonly TimeSpan UninstallTimeout = TimeSpan.FromMinutes(10);

    // ERROR_SUCCESS_REBOOT_REQUIRED, ERROR_SUCCESS_REBOOT_INITIATED
    private static readonly int[] RebootRequiredExitCodes = { 3010, 1641 };

    // MSI uninstall strings often use /I{GUID} (install/repair); /X{GUID} removes the product
    private static readonly Regex MsiInstallSwitch = new(@"(?<=^|\s)[/-]i(?=\s*\{)", RegexOptions.IgnoreCase);
    private static readonly Regex MsiQuietSwitch = new(@"(?<=^|\s)[/-]q", RegexOptions.IgnoreCase);

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

            // For apps that don't support silent uninstall, always use the interactive uninstaller
            var uninstallCommand = item.SupportsSilentUninstall
                ? (item.QuietUninstallString ?? item.UninstallString)
                : item.UninstallString;

            if (string.IsNullOrWhiteSpace(uninstallCommand))
            {
                errors.Add($"{item.DisplayName}: No uninstall command found");
                failed++;
                continue;
            }

            try
            {
                var psi = CreateStartInfo(uninstallCommand, item.SupportsSilentUninstall);
                LogHelper.Log($"Uninstall command: \"{psi.FileName}\" {psi.Arguments}");

                if (!item.SupportsSilentUninstall)
                {
                    // Launch the interactive uninstaller without waiting — user must complete it manually
                    LogHelper.Log($"Launching interactive uninstaller for: {item.DisplayName}");
                    progress?.Report($"{item.DisplayName} requires manual uninstall — launched uninstaller");
                    Process.Start(psi);
                    succeeded++;
                    continue;
                }

                var exitCode = await RunUninstallCommand(psi);
                if (exitCode == 0)
                {
                    LogHelper.Log($"Successfully uninstalled: {item.DisplayName}");
                    succeeded++;
                }
                else if (RebootRequiredExitCodes.Contains(exitCode))
                {
                    LogHelper.Log($"Successfully uninstalled (reboot required): {item.DisplayName}");
                    progress?.Report($"{item.DisplayName} uninstalled — reboot required to finish");
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
            catch (OperationCanceledException)
            {
                var msg = $"{item.DisplayName}: Uninstaller did not finish within " +
                          $"{UninstallTimeout.TotalMinutes:F0} minutes (it may still be running)";
                LogHelper.Log(msg);
                errors.Add(msg);
                failed++;
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

    private static ProcessStartInfo CreateStartInfo(string command, bool silent)
    {
        var (fileName, arguments) = ParseCommand(command);

        if (Path.GetFileNameWithoutExtension(fileName).Equals("msiexec", StringComparison.OrdinalIgnoreCase))
        {
            arguments = MsiInstallSwitch.Replace(arguments, "/X");
            if (silent && !MsiQuietSwitch.IsMatch(arguments))
                arguments += " /qn /norestart";
        }

        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
        };
    }

    private static (string FileName, string Arguments) ParseCommand(string command)
    {
        command = Environment.ExpandEnvironmentVariables(command.Trim());

        if (command.StartsWith('"'))
        {
            var endQuote = command.IndexOf('"', 1);
            return endQuote > 0
                ? (command[1..endQuote], command[(endQuote + 1)..].TrimStart())
                : (command.Trim('"'), string.Empty);
        }

        // Unquoted paths may contain spaces ("C:\Program Files\X\uninst.exe /S").
        // Like CreateProcess, take the shortest space-delimited prefix that is an existing file.
        for (var i = command.IndexOf(' '); i > 0; i = command.IndexOf(' ', i + 1))
        {
            var candidate = command[..i];
            if (File.Exists(candidate) || File.Exists(candidate + ".exe"))
                return (candidate, command[(i + 1)..].TrimStart());
        }

        if (File.Exists(command))
            return (command, string.Empty);

        // Not a full path (e.g. "MsiExec.exe /X{GUID}"): split at the first space
        var spaceIndex = command.IndexOf(' ');
        return spaceIndex > 0
            ? (command[..spaceIndex], command[(spaceIndex + 1)..].TrimStart())
            : (command, string.Empty);
    }

    private static async Task<int> RunUninstallCommand(ProcessStartInfo psi)
    {
        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start uninstall process");

        using var cts = new CancellationTokenSource(UninstallTimeout);
        await process.WaitForExitAsync(cts.Token);
        return process.ExitCode;
    }
}
