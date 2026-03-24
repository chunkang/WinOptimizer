namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Forms;
using WinOptimizer.Helpers;
using WinOptimizer.Models;
using WinOptimizer.Services;

public partial class SoftwareDetectionControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly SoftwareDetectorService _detector = new();
    private readonly UninstallService _uninstaller = new();
    private List<DetectedSoftware> _detectedSoftware = new();

    public SoftwareDetectionControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeComponent();
    }

    public async Task<int> ScanAsync()
    {
        btnScan.Enabled = false;
        btnUninstall.Enabled = false;
        itemsPanel.Controls.Clear();
        _mainForm.SetStatus("Scanning for banking/security software...");
        _mainForm.SetProgress(50);

        try
        {
            _detectedSoftware = await Task.Run(() => _detector.Scan());

            var y = 0;
            foreach (var sw in _detectedSoftware)
            {
                var item = new ModernListItem
                {
                    Text = sw.DisplayName,
                    Publisher = sw.Publisher,
                    Version = sw.DisplayVersion ?? "",
                    IsChecked = true,
                    ItemTag = sw,
                    Location = new Point(0, y),
                    Width = itemsPanel.ClientSize.Width,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                };
                itemsPanel.Controls.Add(item);
                y += item.Height;
            }

            lblCount.Text = $"{_detectedSoftware.Count} program(s) detected";
            _mainForm.SetStatus($"Scan complete. {_detectedSoftware.Count} program(s) found.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Scan failed: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _mainForm.SetStatus("Scan failed.");
        }
        finally
        {
            btnScan.Enabled = true;
            btnUninstall.Enabled = _detectedSoftware.Count > 0;
            _mainForm.SetProgress(100);
        }

        return _detectedSoftware.Count;
    }

    private async void BtnScan_Click(object? sender, EventArgs e)
    {
        var count = await ScanAsync();
        _mainForm.UpdateTabBadge(0, count > 0 ? $"{count} found" : null);
    }

    public async Task<(int succeeded, int failed, List<string> errors)> UninstallAllAsync(IProgress<string> progress)
    {
        if (_detectedSoftware.Count == 0)
            return (0, 0, new List<string>());

        btnScan.Enabled = false;
        btnUninstall.Enabled = false;

        try
        {
            return await _uninstaller.UninstallSelected(_detectedSoftware, progress);
        }
        finally
        {
            btnScan.Enabled = true;
            btnUninstall.Enabled = true;
        }
    }

    public List<DetectedSoftware> GetDetectedSoftware() => _detectedSoftware;

    private void BtnSelectAll_Click(object? sender, EventArgs e)
    {
        foreach (var ctrl in itemsPanel.Controls.OfType<ModernListItem>())
            ctrl.IsChecked = true;
    }

    private void BtnDeselectAll_Click(object? sender, EventArgs e)
    {
        foreach (var ctrl in itemsPanel.Controls.OfType<ModernListItem>())
            ctrl.IsChecked = false;
    }

    private async void BtnUninstall_Click(object? sender, EventArgs e)
    {
        var selected = itemsPanel.Controls.OfType<ModernListItem>()
            .Where(i => i.IsChecked)
            .Select(i => (DetectedSoftware)i.ItemTag!)
            .ToList();

        if (selected.Count == 0)
        {
            _mainForm.SetStatus("No programs selected.");
            return;
        }

        RestorePointService.CreateRestorePoint("WinOptimizer - Before software removal");

        btnScan.Enabled = false;
        btnUninstall.Enabled = false;
        _mainForm.SetStatus("Uninstalling selected software...");

        var progress = new Progress<string>(msg => _mainForm.SetStatus(msg));
        var (succeeded, failed, errors) = await _uninstaller.UninstallSelected(selected, progress);

        var summary = $"Uninstall complete: {succeeded} succeeded, {failed} failed.";
        if (errors.Count > 0)
            summary += "\n\nErrors:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));
        LogHelper.Log(summary);

        _mainForm.SetStatus(summary.Split('\n')[0]);
        btnScan.Enabled = true;
        btnUninstall.Enabled = true;

        var count = await ScanAsync();
        _mainForm.UpdateTabBadge(0, count > 0 ? $"{count} found" : null);
    }
}
