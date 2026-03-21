namespace WinOptimizer.Controls;

using WinOptimizer.Forms;
using WinOptimizer.Models;
using WinOptimizer.Services;

public partial class BrowserCacheCleanupControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly BrowserCacheCleanupService _service = new();
    private List<BrowserCacheInfo> _browsers = new();

    public BrowserCacheCleanupControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeComponent();
        ScanBrowsers();
    }

    private void ScanBrowsers()
    {
        checkedListBox.Items.Clear();
        _browsers = _service.DetectBrowsers();

        if (_browsers.Count == 0)
        {
            checkedListBox.Items.Add("No browser caches detected.");
            btnClean.Enabled = false;
            return;
        }

        btnClean.Enabled = true;
        foreach (var browser in _browsers)
        {
            var runningTag = browser.IsRunning ? " [RUNNING]" : "";
            var label = $"{browser.BrowserName} — {browser.CacheSizeDisplay}{runningTag}";
            var index = checkedListBox.Items.Add(label);
            checkedListBox.SetItemChecked(index, !browser.IsRunning);
        }
    }

    private void BtnScan_Click(object? sender, EventArgs e)
    {
        _mainForm.SetStatus("Scanning browser caches...");
        ScanBrowsers();
        _mainForm.SetStatus($"Found {_browsers.Count} browser(s) with cache data.");
    }

    private void BtnClean_Click(object? sender, EventArgs e)
    {
        var selected = new List<BrowserCacheInfo>();
        for (int i = 0; i < checkedListBox.Items.Count && i < _browsers.Count; i++)
        {
            if (checkedListBox.GetItemChecked(i))
                selected.Add(_browsers[i]);
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
