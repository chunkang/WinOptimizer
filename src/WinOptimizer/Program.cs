// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer;

using WinOptimizer.Helpers;
using WinOptimizer.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        if (!AdminHelper.IsRunningAsAdmin())
        {
            AdminHelper.RestartAsAdmin();
            return;
        }

        LogHelper.Initialize();
        LogHelper.Log("WinOptimizer started");

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, args) =>
        {
            LogHelper.Log($"UI thread exception: {args.Exception}");
            MessageBox.Show(args.Exception.ToString(), "WinOptimizer Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LogHelper.Log($"Unhandled exception: {ex}");
            MessageBox.Show(ex?.ToString() ?? "Unknown error", "WinOptimizer Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        try
        {
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Fatal exception: {ex}");
            MessageBox.Show(ex.ToString(), "WinOptimizer Fatal Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
