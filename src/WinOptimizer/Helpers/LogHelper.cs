// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Helpers;

public static class LogHelper
{
    private static string _logFilePath = string.Empty;
    private static readonly object _lock = new();

    public static void Initialize()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinOptimizer", "logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, "winoptimizer.log");
    }

    public static void Log(string message)
    {
        if (string.IsNullOrEmpty(_logFilePath)) return;

        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, entry + Environment.NewLine);
            }
            catch
            {
                // Silently ignore logging failures
            }
        }
    }
}
