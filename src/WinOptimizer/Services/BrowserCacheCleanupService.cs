namespace WinOptimizer.Services;

using System.Diagnostics;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class BrowserCacheCleanupService
{
    // Chromium-based browsers: (Name, ProcessName, UserDataRelativePath, UseRoamingAppData)
    private static readonly (string Name, string ProcessName, string UserDataPath, bool UseRoaming)[] ChromiumBrowsers =
    {
        ("Microsoft Edge", "msedge", @"Microsoft\Edge\User Data", false),
        ("Google Chrome", "chrome", @"Google\Chrome\User Data", false),
        ("Brave", "brave", @"BraveSoftware\Brave-Browser\User Data", false),
        ("Opera", "opera", @"Opera Software\Opera Stable", true),
    };

    // Cache subdirectories to scan within each profile
    private static readonly string[] ChromiumCacheSubDirs =
    {
        @"Cache\Cache_Data",
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
            var baseDir = useRoaming
                ? Path.Combine(roamingAppData, userDataPath)
                : Path.Combine(localAppData, userDataPath);

            var cachePaths = GetChromiumCachePaths(baseDir, name == "Opera");

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

        // Detect Firefox
        var firefoxPaths = GetFirefoxCachePaths(localAppData, roamingAppData);
        if (firefoxPaths.Any(Directory.Exists))
        {
            var totalSize = firefoxPaths.Where(Directory.Exists).Sum(p => GetDirectorySize(p));
            var isRunning = Process.GetProcessesByName("firefox").Length > 0;

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
