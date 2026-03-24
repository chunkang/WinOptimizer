namespace WinOptimizer.Models;

public class CleanupTask
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsCleanable => SizeBytes > 0;
    public bool IsSelected { get; set; }
    public CleanupType Type { get; set; }
}

public enum CleanupType
{
    RecycleBin,
    WindowsTemp,
    UserTemp,
}
