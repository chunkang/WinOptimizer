// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Claude (AI-assisted)
// Modified: Claude (AI-assisted) (2026-10-07)
// ============================================================================

namespace WinOptimizer.Helpers;

// Directory walks go one level at a time so that a single inaccessible
// directory or locked file is skipped instead of aborting the whole walk.
public static class FileSystemHelper
{
    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path)) return 0;
        return GetDirectorySizeRecursive(new DirectoryInfo(path));
    }

    public static void DeleteDirectoryContents(string path)
    {
        if (!Directory.Exists(path)) return;

        var dir = new DirectoryInfo(path);
        DeleteFilesRecursive(dir);
        DeleteEmptyDirectoriesRecursive(dir);
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

    private static void DeleteFilesRecursive(DirectoryInfo dir)
    {
        try
        {
            foreach (var file in dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                try { file.Delete(); }
                catch { /* Skip locked/in-use files */ }
            }
        }
        catch { /* Skip inaccessible directory */ }

        try
        {
            foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                DeleteFilesRecursive(subDir);
            }
        }
        catch { /* Skip inaccessible directory */ }
    }

    // Removes empty subdirectories deepest-first; the root directory itself is kept
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
}
