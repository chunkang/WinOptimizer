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

                freedBytes += task.SizeBytes;
                cleaned++;
                LogHelper.Log($"Cleaned: {task.Name} ({task.SizeBytes} bytes)");
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

    private static long GetDirectorySize(string path)
    {
        try
        {
            return new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f =>
                {
                    try { return f.Length; }
                    catch { return 0L; }
                });
        }
        catch
        {
            return 0;
        }
    }

    public static string FormatBytes(long bytes) => BrowserCacheCleanupService.FormatBytes(bytes);
}
