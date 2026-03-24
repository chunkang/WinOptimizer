namespace WinOptimizer.Services;

using System.Runtime.InteropServices;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class SystemCleanupService
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string? pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    public List<CleanupTask> Scan()
    {
        var tasks = new List<CleanupTask>
        {
            ScanRecycleBin(),
            ScanTempDirectory(CleanupType.WindowsTemp),
            ScanTempDirectory(CleanupType.UserTemp),
        };
        return tasks;
    }

    public (int cleaned, long freedBytes, List<string> errors) Clean(IEnumerable<CleanupTask> tasks)
    {
        var cleaned = 0;
        var freedBytes = 0L;
        var errors = new List<string>();

        foreach (var task in tasks)
        {
            if (!task.IsCleanable) continue;

            try
            {
                var sizeBefore = task.SizeBytes;

                switch (task.Type)
                {
                    case CleanupType.RecycleBin:
                        CleanRecycleBin();
                        break;
                    case CleanupType.WindowsTemp:
                    case CleanupType.UserTemp:
                        CleanTempDirectory(task.Type);
                        break;
                }

                // Measure actual freed space by re-scanning
                var sizeAfter = task.Type == CleanupType.RecycleBin
                    ? GetRecycleBinSize()
                    : GetTempDirectorySize(task.Type);
                var actualFreed = sizeBefore - sizeAfter;
                if (actualFreed > 0)
                    freedBytes += actualFreed;

                cleaned++;
                LogHelper.Log($"Cleaned: {task.Name} (freed {actualFreed} of {sizeBefore} bytes, {sizeAfter} bytes remain)");
            }
            catch (Exception ex)
            {
                errors.Add($"{task.Name}: {ex.Message}");
                LogHelper.Log($"Error cleaning {task.Name}: {ex.Message}");
            }
        }

        return (cleaned, freedBytes, errors);
    }

    private static CleanupTask ScanRecycleBin()
    {
        var task = new CleanupTask
        {
            Name = "Empty Recycle Bin",
            Description = "Remove all items from the Recycle Bin",
            Type = CleanupType.RecycleBin,
        };

        try
        {
            var info = new SHQUERYRBINFO { cbSize = Marshal.SizeOf<SHQUERYRBINFO>() };
            var hr = SHQueryRecycleBin(null, ref info);
            if (hr == 0)
                task.SizeBytes = info.i64Size;
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error scanning Recycle Bin: {ex.Message}");
        }

        return task;
    }

    private static CleanupTask ScanTempDirectory(CleanupType type)
    {
        var path = type == CleanupType.WindowsTemp
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
            : Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

        var task = new CleanupTask
        {
            Name = type == CleanupType.WindowsTemp ? "Clean Windows Temp" : "Clean User Temp",
            Description = $"Delete temporary files in {path}",
            Type = type,
        };

        try
        {
            if (Directory.Exists(path))
                task.SizeBytes = GetDirectorySize(path);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error scanning temp directory {path}: {ex.Message}");
        }

        return task;
    }

    private static void CleanRecycleBin()
    {
        var hr = SHEmptyRecycleBin(IntPtr.Zero, null,
            SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
        // S_OK = 0, S_FALSE = 1 (already empty) are both acceptable
        if (hr != 0 && hr != 1)
            Marshal.ThrowExceptionForHR(hr);
    }

    private static void CleanTempDirectory(CleanupType type)
    {
        var path = type == CleanupType.WindowsTemp
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
            : Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

        if (!Directory.Exists(path)) return;

        var dir = new DirectoryInfo(path);

        // Delete files — use TopDirectoryOnly per directory to avoid
        // UnauthorizedAccessException aborting the entire enumeration
        DeleteFilesRecursive(dir);

        // Remove empty directories deepest-first
        DeleteEmptyDirectoriesRecursive(dir);
    }

    private static void DeleteFilesRecursive(DirectoryInfo dir)
    {
        // Delete files in this directory
        try
        {
            foreach (var file in dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                try { file.Delete(); }
                catch { /* Skip locked/in-use files */ }
            }
        }
        catch { /* Skip inaccessible directory */ }

        // Recurse into subdirectories individually
        try
        {
            foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                DeleteFilesRecursive(subDir);
            }
        }
        catch { /* Skip inaccessible directory */ }
    }

    private static void DeleteEmptyDirectoriesRecursive(DirectoryInfo dir)
    {
        try
        {
            foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                DeleteEmptyDirectoriesRecursive(subDir);

                try
                {
                    if (!subDir.EnumerateFileSystemInfos().Any())
                        subDir.Delete();
                }
                catch { /* Skip locked directories */ }
            }
        }
        catch { /* Skip inaccessible directory */ }
    }

    private static long GetRecycleBinSize()
    {
        try
        {
            var info = new SHQUERYRBINFO { cbSize = Marshal.SizeOf<SHQUERYRBINFO>() };
            var hr = SHQueryRecycleBin(null, ref info);
            return hr == 0 ? info.i64Size : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static long GetTempDirectorySize(CleanupType type)
    {
        var path = type == CleanupType.WindowsTemp
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
            : Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        return GetDirectorySize(path);
    }

    private static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path)) return 0;
        return GetDirectorySizeRecursive(new DirectoryInfo(path));
    }

    private static long GetDirectorySizeRecursive(DirectoryInfo dir)
    {
        var size = 0L;

        try
        {
            foreach (var file in dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                try { size += file.Length; }
                catch { /* Skip inaccessible files */ }
            }
        }
        catch { /* Skip inaccessible directory */ }

        try
        {
            foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                size += GetDirectorySizeRecursive(subDir);
            }
        }
        catch { /* Skip inaccessible directory */ }

        return size;
    }

    public static string FormatBytes(long bytes) => BrowserCacheCleanupService.FormatBytes(bytes);
}
