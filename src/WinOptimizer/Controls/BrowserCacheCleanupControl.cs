// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Johnny Kang <abjohnkang@gmail.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

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

    // Load data from service (can run on background thread)
    public List<BrowserCacheInfo> DetectBrowserData()
    {
        _browsers = _service.DetectBrowsers();
        return _browsers;
    }

    // Populate UI from loaded data (must run on UI thread)
    public void PopulateUI()
    {
        itemsPanel.SuspendLayout();
        itemsPanel.Controls.Clear();

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
            return;
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
    }

    public (int count, long totalBytes) ScanBrowsers()
    {
        DetectBrowserData();
        PopulateUI();
        return (_browsers.Count, _browsers.Sum(b => b.CacheSizeBytes));
    }

    private async void BtnScan_Click(object? sender, EventArgs e)
    {
        btnScan.Enabled = false;
        _mainForm.SetStatus("Scanning browser caches...");
        _mainForm.SetProgress(30);
        await Task.Run(() => DetectBrowserData());
        PopulateUI();
        var totalBytes = _browsers.Sum(b => b.CacheSizeBytes);
        _mainForm.SetStatus($"Found {_browsers.Count} browser(s) with cache data.");
        _mainForm.SetProgress(100);
        var badge = totalBytes > 0 ? BrowserCacheCleanupService.FormatBytes(totalBytes) : null;
        _mainForm.UpdateTabBadge(3, badge);
        btnScan.Enabled = true;
    }

    public List<BrowserCacheInfo> GetCleanableBrowsers() =>
        _browsers.Where(b => b.CacheSizeBytes > 0).ToList();

    public (int cleaned, long freedBytes, List<string> errors) CleanAll()
    {
        var cleanable = GetCleanableBrowsers();
        if (cleanable.Count == 0)
            return (0, 0, new List<string>());

        var result = _service.CleanCache(cleanable);
        ScanBrowsers();
        return result;
    }

    private async void BtnClean_Click(object? sender, EventArgs e)
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

        var totalSize = selected.Sum(b => b.CacheSizeBytes);
        var browserList = string.Join("\n", selected.Select(b => $"  - {b.BrowserName} ({b.CacheSizeDisplay})"));

        var runningBrowsers = selected.Where(b => b.IsRunning).ToList();
        var runningWarning = runningBrowsers.Count > 0
            ? $"\n\nThe following browsers are running and will be terminated:\n" +
              string.Join("\n", runningBrowsers.Select(b => $"  - {b.BrowserName}"))
            : "";

        var confirm = MessageBox.Show(
            $"The following browser caches will be deleted:\n\n{browserList}\n\n" +
            $"Total: ~{BrowserCacheCleanupService.FormatBytes(totalSize)}\n\n" +
            "This will not affect bookmarks, saved passwords, or extensions." +
            runningWarning + "\n\nContinue?",
            "Confirm Cache Cleanup",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        btnClean.Enabled = false;
        btnScan.Enabled = false;
        _mainForm.SetStatus("Cleaning browser caches...");
        _mainForm.SetProgress(30);

        var (cleaned, freedBytes, errors) = await Task.Run(() => _service.CleanCache(selected));

        _mainForm.SetProgress(80);
        var msg = $"Cleaned {cleaned} browser cache(s), freed {BrowserCacheCleanupService.FormatBytes(freedBytes)}.";
        if (errors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", errors.Select(err => $"  - {err}"));

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        _mainForm.SetProgress(90);
        await Task.Run(() => DetectBrowserData());
        PopulateUI();
        _mainForm.SetProgress(100);
        btnClean.Enabled = true;
        btnScan.Enabled = true;
    }
}
