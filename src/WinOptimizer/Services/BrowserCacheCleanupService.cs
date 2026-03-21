namespace WinOptimizer.Services;

using System.Diagnostics;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class BrowserCacheCleanupService
{
    private static readonly (string Name, string ProcessName, string[] RelativePaths)[] Browsers =
    {
        ("Microsoft Edge", "msedge", new[]
        {
            @"Microsoft\Edge\User Data\Default\Cache",
            @"Microsoft\Edge\User Data\Default\Code Cache",
            @"Microsoft\Edge\User Data\Default\Service Worker\CacheStorage",
        }),
        ("Google Chrome", "chrome", new[]
        {
            @"Google\Chrome\User Data\Default\Cache",
            @"Google\Chrome\User Data\Default\Code Cache",
            @"Google\Chrome\User Data\Default\Service Worker\CacheStorage",
        }),
        ("Mozilla Firefox", "firefox", new[]
        {
            // Firefox uses profile-based paths; handled separately
        }),
        ("Opera", "opera", new[]
        {
            @"Opera Software\Opera Stable\Cache",
            @"Opera Software\Opera Stable\Code Cache",
        }),
        ("Brave", "brave", new[]
        {
            @"BraveSoftware\Brave-Browser\User Data\Default\Cache",
            @"BraveSoftware\Brave-Browser\User Data\Default\Code Cache",
            @"BraveSoftware\Brave-Browser\User Data\Default\Service Worker\CacheStorage",
        }),
    };

    public List<BrowserCacheInfo> DetectBrowsers()
    {
        var results = new List<BrowserCacheInfo>();
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        foreach (var (name, processName, relativePaths) in Browsers)
        {
            var cachePaths = name == "Mozilla Firefox"
                ? GetFirefoxCachePaths(localAppData, roamingAppData)
                : relativePaths.Select(p =>
                    name == "Opera"
                        ? Path.Combine(roamingAppData, p)
                        : Path.Combine(localAppData, p)).ToList();

            var totalSize = 0L;
            var anyExists = false;
            foreach (var path in cachePaths)
            {
                if (Directory.Exists(path))
                {
                    anyExists = true;
                    totalSize += GetDirectorySize(path);
                }
            }

            if (!anyExists) continue;

            var isRunning = Process.GetProcessesByName(processName).Length > 0;

            results.Add(new BrowserCacheInfo
            {
                BrowserName = name,
                CachePath = string.Join(";", cachePaths.Where(Directory.Exists)),
                CacheSizeBytes = totalSize,
                IsInstalled = true,
                IsRunning = isRunning,
            });
        }

        return results;
    }

    public (int cleaned, long freedBytes, List<string> errors) CleanCache(IEnumerable<BrowserCacheInfo> browsers)
    {
        var cleaned = 0;
        var freedBytes = 0L;
        var errors = new List<string>();

        foreach (var browser in browsers)
        {
            if (browser.IsRunning)
            {
                errors.Add($"{browser.BrowserName}: Browser is running. Close it first.");
                continue;
            }

            var paths = browser.CachePath.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var browserFreed = 0L;
            var hadError = false;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path)) continue;

                try
                {
                    var size = GetDirectorySize(path);
                    DeleteDirectoryContents(path);
                    browserFreed += size;
                    LogHelper.Log($"Cleaned cache: {browser.BrowserName} - {path} ({size} bytes)");
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

    private static long GetDirectorySize(string path)
    {
        try
        {
            return new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => f.Length);
        }
        catch
        {
            return 0;
        }
    }

    private static void DeleteDirectoryContents(string path)
    {
        var dir = new DirectoryInfo(path);

        foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            try { file.Delete(); }
            catch { /* Skip locked files */ }
        }

        foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.AllDirectories)
                     .OrderByDescending(d => d.FullName.Length))
        {
            try
            {
                if (!subDir.EnumerateFileSystemInfos().Any())
                    subDir.Delete();
            }
            catch { /* Skip locked directories */ }
        }
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
