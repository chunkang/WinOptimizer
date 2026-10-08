// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Johnny Kang <abjohnkang@gmail.com>
// Modified: Claude (AI-assisted) (2026-10-07)
// ============================================================================

namespace WinOptimizer.Services;

using System.Diagnostics;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class BrowserCacheCleanupService
{
    // Chromium-based browsers: (Name, ProcessName, UserDataRelativePath, UseRoamingAppData).
    // Opera keeps its profile under Roaming but its HTTP cache under Local, so both are scanned.
    private static readonly (string Name, string ProcessName, string UserDataPath, bool UseRoaming)[] ChromiumBrowsers =
    {
        ("Microsoft Edge", "msedge", @"Microsoft\Edge\User Data", false),
        ("Google Chrome", "chrome", @"Google\Chrome\User Data", false),
        ("Brave", "brave", @"BraveSoftware\Brave-Browser\User Data", false),
        ("Opera", "opera", @"Opera Software\Opera Stable", true),
    };

    // Cache subdirectories to scan within each profile. These must not nest
    // (e.g. "Cache" already includes "Cache\Cache_Data"), or sizes are counted twice.
    private static readonly string[] ChromiumCacheSubDirs =
    {
        "Cache",
        "Code Cache",
        @"Service Worker\CacheStorage",
    };

    public List<BrowserCacheInfo> DetectBrowsers()
    {
        var results = new List<BrowserCacheInfo>();
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Detect Chromium-based browsers
        foreach (var (name, processName, userDataPath, useRoaming) in ChromiumBrowsers)
        {
            var baseDirs = useRoaming
                ? new[] { Path.Combine(roamingAppData, userDataPath), Path.Combine(localAppData, userDataPath) }
                : new[] { Path.Combine(localAppData, userDataPath) };

            var cachePaths = baseDirs
                .SelectMany(dir => GetChromiumCachePaths(dir, name == "Opera"))
                .ToList();

            var totalSize = 0L;
            var anyExists = false;
            foreach (var path in cachePaths)
            {
                if (Directory.Exists(path))
                {
                    anyExists = true;
                    totalSize += FileSystemHelper.GetDirectorySize(path);
                }
            }

            if (!anyExists) continue;

            var isRunning = IsProcessRunning(processName);

            results.Add(new BrowserCacheInfo
            {
                BrowserName = name,
                CachePath = string.Join(";", cachePaths.Where(Directory.Exists)),
                CacheSizeBytes = totalSize,
                IsInstalled = true,
                IsRunning = isRunning,
            });
        }

        // Detect Firefox
        var firefoxPaths = GetFirefoxCachePaths(localAppData, roamingAppData);
        if (firefoxPaths.Any(Directory.Exists))
        {
            var totalSize = firefoxPaths.Where(Directory.Exists).Sum(p => FileSystemHelper.GetDirectorySize(p));
            var isRunning = IsProcessRunning("firefox");

            results.Add(new BrowserCacheInfo
            {
                BrowserName = "Mozilla Firefox",
                CachePath = string.Join(";", firefoxPaths.Where(Directory.Exists)),
                CacheSizeBytes = totalSize,
                IsInstalled = true,
                IsRunning = isRunning,
            });
        }

        results.Sort((a, b) => string.Compare(a.BrowserName, b.BrowserName, StringComparison.OrdinalIgnoreCase));
        return results;
    }

    private static readonly Dictionary<string, string> BrowserProcessNames = ChromiumBrowsers
        .ToDictionary(b => b.Name, b => b.ProcessName);

    static BrowserCacheCleanupService()
    {
        BrowserProcessNames["Mozilla Firefox"] = "firefox";
    }

    public static bool IsBrowserRunning(string browserName) =>
        BrowserProcessNames.TryGetValue(browserName, out var processName) && IsProcessRunning(processName);

    private static bool IsProcessRunning(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        foreach (var proc in processes)
            proc.Dispose();
        return processes.Length > 0;
    }

    public static void KillBrowserProcess(string browserName)
    {
        if (!BrowserProcessNames.TryGetValue(browserName, out var processName))
            return;

        try
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var proc in processes)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit(5000);
                    LogHelper.Log($"Terminated {browserName} process (PID {proc.Id})");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Failed to terminate {browserName} (PID {proc.Id}): {ex.Message}");
                }
                finally
                {
                    proc.Dispose();
                }
            }

            // Brief delay to allow file handles to be released
            if (processes.Length > 0)
                Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error killing {browserName} processes: {ex.Message}");
        }
    }

    private static List<string> GetChromiumCachePaths(string baseDir, bool isFlat)
    {
        var paths = new List<string>();

        if (!Directory.Exists(baseDir)) return paths;

        if (isFlat)
        {
            // Opera stores cache directly under the base dir (no profile subdirectories)
            foreach (var subDir in ChromiumCacheSubDirs)
                paths.Add(Path.Combine(baseDir, subDir));
        }
        else
        {
            // Scan all profile directories: Default, Profile 1, Profile 2, Guest Profile, System Profile, etc.
            try
            {
                foreach (var profileDir in Directory.GetDirectories(baseDir))
                {
                    var dirName = Path.GetFileName(profileDir);
                    if (dirName.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
                        dirName.StartsWith("Profile ", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("Guest Profile", StringComparison.OrdinalIgnoreCase) ||
                        dirName.Equals("System Profile", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var subDir in ChromiumCacheSubDirs)
                            paths.Add(Path.Combine(profileDir, subDir));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error scanning profiles in {baseDir}: {ex.Message}");
            }
        }

        return paths;
    }

    public (int cleaned, long freedBytes, List<string> errors) CleanCache(IEnumerable<BrowserCacheInfo> browsers)
    {
        var cleaned = 0;
        var freedBytes = 0L;
        var errors = new List<string>();

        foreach (var browser in browsers)
        {
            // IsRunning is refreshed by the caller right before confirming, so only
            // browsers the user was warned about get closed
            if (browser.IsRunning)
                KillBrowserProcess(browser.BrowserName);

            var paths = browser.CachePath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var browserFreed = 0L;
            var hadError = false;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path)) continue;

                try
                {
                    // Measure before and after: locked files are skipped and must not count as freed
                    var sizeBefore = FileSystemHelper.GetDirectorySize(path);
                    FileSystemHelper.DeleteDirectoryContents(path);
                    var freed = Math.Max(0, sizeBefore - FileSystemHelper.GetDirectorySize(path));
                    browserFreed += freed;
                    LogHelper.Log($"Cleaned cache: {browser.BrowserName} - {path} (freed {freed} of {sizeBefore} bytes)");
                }
                catch (Exception ex)
                {
                    hadError = true;
                    errors.Add($"{browser.BrowserName}: {ex.Message}");
                    LogHelper.Log($"Error cleaning {browser.BrowserName} cache at {path}: {ex.Message}");
                }
            }

            if (browserFreed > 0 || !hadError)
            {
                cleaned++;
                freedBytes += browserFreed;
            }
        }

        return (cleaned, freedBytes, errors);
    }

    private static List<string> GetFirefoxCachePaths(string localAppData, string roamingAppData)
    {
        var paths = new List<string>();
        var profilesDir = Path.Combine(roamingAppData, @"Mozilla\Firefox\Profiles");
        if (!Directory.Exists(profilesDir)) return paths;

        try
        {
            foreach (var profileDir in Directory.GetDirectories(profilesDir))
            {
                var cache2 = Path.Combine(localAppData, @"Mozilla\Firefox\Profiles",
                    Path.GetFileName(profileDir), "cache2");
                if (Directory.Exists(cache2))
                    paths.Add(cache2);
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error scanning Firefox profiles: {ex.Message}");
        }

        return paths;
    }

    public static string FormatBytes(long bytes) =>
        bytes switch
        {
            >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F2} GB",
            >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
            >= 1024 => $"{bytes / 1024.0:F0} KB",
            _ => $"{bytes} bytes"
        };
}
