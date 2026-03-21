namespace WinOptimizer.Models;

public class BrowserCacheInfo
{
    public string BrowserName { get; set; } = string.Empty;
    public string CachePath { get; set; } = string.Empty;
    public long CacheSizeBytes { get; set; }
    public bool IsInstalled { get; set; }
    public bool IsRunning { get; set; }

    public string CacheSizeDisplay =>
        CacheSizeBytes switch
        {
            >= 1_073_741_824 => $"{CacheSizeBytes / 1_073_741_824.0:F2} GB",
            >= 1_048_576 => $"{CacheSizeBytes / 1_048_576.0:F1} MB",
            >= 1024 => $"{CacheSizeBytes / 1024.0:F0} KB",
            _ => $"{CacheSizeBytes} bytes"
        };
}
