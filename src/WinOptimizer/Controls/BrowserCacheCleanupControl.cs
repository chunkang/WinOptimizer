namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Forms;
using WinOptimizer.Models;
using WinOptimizer.Services;
using WinOptimizer.Theme;

public partial class BrowserCacheCleanupControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly BrowserCacheCleanupService _service = new();
    private List<BrowserCacheInfo> _browsers = new();

    public BrowserCacheCleanupControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeComponent();
    }

    public (int count, long totalBytes) ScanBrowsers()
    {
        itemsPanel.SuspendLayout();
        itemsPanel.Controls.Clear();
        _browsers = _service.DetectBrowsers();

        if (_browsers.Count == 0)
        {
            var noDataLabel = new Label
            {
                Text = "No browser caches detected.",
                Font = AppTheme.CardBodyFont,
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(0, 8),
                AutoSize = true,
            };
            itemsPanel.Controls.Add(noDataLabel);
            btnClean.Enabled = false;
            itemsPanel.ResumeLayout();
            return (0, 0);
        }

        btnClean.Enabled = true;
        var y = 0;
        foreach (var browser in _browsers)
        {
            var runningTag = browser.IsRunning ? " [RUNNING]" : "";
            var item = new ModernCheckItem
            {
                Text = $"{browser.BrowserName}{runningTag}",
                Description = $"Cache size: {browser.CacheSizeDisplay}",
                IsChecked = !browser.IsRunning,
                StatusText = browser.IsRunning ? "Running" : browser.CacheSizeDisplay,
                StatusColor = browser.IsRunning ? AppTheme.StatusRed : AppTheme.StatusAmber,
                ItemTag = browser,
                Height = 54,
                Location = new Point(0, y),
                Width = itemsPanel.ClientSize.Width,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            itemsPanel.Controls.Add(item);
            y += item.Height;
        }

        itemsPanel.ResumeLayout();
        return (_browsers.Count, _browsers.Sum(b => b.CacheSizeBytes));
    }

    private void BtnScan_Click(object? sender, EventArgs e)
    {
        _mainForm.SetStatus("Scanning browser caches...");
        var (count, totalBytes) = ScanBrowsers();
        _mainForm.SetStatus($"Found {count} browser(s) with cache data.");
        var badge = totalBytes > 0 ? BrowserCacheCleanupService.FormatBytes(totalBytes) : null;
        _mainForm.UpdateTabBadge(3, badge);
    }

    public List<BrowserCacheInfo> GetCleanableBrowsers() =>
        _browsers.Where(b => !b.IsRunning && b.CacheSizeBytes > 0).ToList();

    public (int cleaned, long freedBytes, List<string> errors) CleanAll()
    {
        var cleanable = GetCleanableBrowsers();
        if (cleanable.Count == 0)
            return (0, 0, new List<string>());

        var result = _service.CleanCache(cleanable);
        ScanBrowsers();
        return result;
    }

    private void BtnClean_Click(object? sender, EventArgs e)
    {
        var selected = new List<BrowserCacheInfo>();
        foreach (var ctrl in itemsPanel.Controls.OfType<ModernCheckItem>())
        {
            if (ctrl.IsChecked && ctrl.ItemTag is BrowserCacheInfo b)
                selected.Add(b);
        }

        if (selected.Count == 0)
        {
            MessageBox.Show("No browsers selected for cleanup.", "WinOptimizer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var runningBrowsers = selected.Where(b => b.IsRunning).ToList();
        if (runningBrowsers.Count > 0)
        {
            var names = string.Join(", ", runningBrowsers.Select(b => b.BrowserName));
            MessageBox.Show(
                $"The following browsers are still running and must be closed first:\n\n{names}\n\nPlease close them and try again.",
                "Browsers Running",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var totalSize = selected.Sum(b => b.CacheSizeBytes);
        var browserList = string.Join("\n", selected.Select(b => $"  - {b.BrowserName} ({b.CacheSizeDisplay})"));
        var confirm = MessageBox.Show(
            $"The following browser caches will be deleted:\n\n{browserList}\n\n" +
            $"Total: ~{BrowserCacheCleanupService.FormatBytes(totalSize)}\n\n" +
            "This will not affect bookmarks, saved passwords, or extensions.\n\nContinue?",
            "Confirm Cache Cleanup",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        _mainForm.SetStatus("Cleaning browser caches...");
        var (cleaned, freedBytes, errors) = _service.CleanCache(selected);

        var msg = $"Cleaned {cleaned} browser cache(s), freed {BrowserCacheCleanupService.FormatBytes(freedBytes)}.";
        if (errors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        ScanBrowsers();
    }
}
