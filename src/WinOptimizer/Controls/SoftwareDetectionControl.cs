// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-10-07)
// ============================================================================

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
                var displayText = sw.SupportsSilentUninstall
                    ? sw.DisplayName
                    : $"{sw.DisplayName} (manual uninstall)";

                var item = new ModernListItem
                {
                    Text = displayText,
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

    public async Task<(int succeeded, int failed, List<string> errors)> UninstallAsync(
        List<DetectedSoftware> selected, IProgress<string> progress)
    {
        if (selected.Count == 0)
            return (0, 0, new List<string>());

        btnScan.Enabled = false;
        btnUninstall.Enabled = false;

        try
        {
            return await _uninstaller.UninstallSelected(selected, progress);
        }
        finally
        {
            btnScan.Enabled = true;
            btnUninstall.Enabled = true;
        }
    }

    // Only the items the user left checked; detection can match unrelated software
    public List<DetectedSoftware> GetSelectedSoftware() =>
        itemsPanel.Controls.OfType<ModernListItem>()
            .Where(i => i.IsChecked)
            .Select(i => (DetectedSoftware)i.ItemTag!)
            .ToList();

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
        var selected = GetSelectedSoftware();

        if (selected.Count == 0)
        {
            _mainForm.SetStatus("No programs selected.");
            return;
        }

        var programList = string.Join("\n", selected.Select(s => $"  - {s.DisplayName}"));
        var confirm = MessageBox.Show(
            $"The following programs will be uninstalled:\n\n{programList}\n\nContinue?",
            "Confirm Uninstall",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        btnScan.Enabled = false;
        btnUninstall.Enabled = false;

        try
        {
            if (!await RestorePointService.PromptAndCreateAsync("WinOptimizer - Before software removal"))
                return;

            _mainForm.SetStatus("Uninstalling selected software...");

            var progress = new Progress<string>(msg => _mainForm.SetStatus(msg));
            var (succeeded, failed, errors) = await _uninstaller.UninstallSelected(selected, progress);

            var summary = $"Uninstall complete: {succeeded} succeeded, {failed} failed.";
            if (errors.Count > 0)
                summary += "\n\nErrors:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));
            LogHelper.Log(summary);

            MessageBox.Show(summary, "Results",
                MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            _mainForm.SetStatus(summary.Split('\n')[0]);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Uninstall error: {ex}");
            MessageBox.Show($"Uninstall failed: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _mainForm.SetStatus("Uninstall failed.");
        }
        finally
        {
            btnScan.Enabled = true;
            btnUninstall.Enabled = _detectedSoftware.Count > 0;
        }

        var count = await ScanAsync();
        _mainForm.UpdateTabBadge(0, count > 0 ? $"{count} found" : null);
    }
}
