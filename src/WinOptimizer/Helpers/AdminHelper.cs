// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Helpers;

using System.Diagnostics;
using System.Security.Principal;

public static class AdminHelper
{
    public static bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void RestartAsAdmin()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath ?? Application.ExecutablePath,
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"WinOptimizer requires administrator privileges to run.\n\n{ex.Message}",
                "WinOptimizer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
