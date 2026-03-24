// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Forms;
using WinOptimizer.Models;
using WinOptimizer.Services;
using WinOptimizer.Theme;

public partial class NetworkOptimizationControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly NetworkOptimizerService _optimizer = new();
    private List<NetworkSetting> _settings = new();

    public NetworkOptimizationControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeComponent();
    }

    // Load data from services (can run on background thread)
    public int LoadSettingsData()
    {
        _settings = _optimizer.GetSettings();
        return _settings.Count(s => !s.IsApplied);
    }

    // Populate UI from loaded data (must run on UI thread)
    public void PopulateUI()
    {
        itemsPanel.SuspendLayout();
        itemsPanel.Controls.Clear();

        var y = 0;
        foreach (var setting in _settings)
        {
            var item = new ModernCheckItem
            {
                Text = setting.Name,
                Description = setting.Description,
                IsChecked = !setting.IsApplied,
                StatusText = setting.IsApplied ? "Applied" : "",
                StatusColor = setting.IsApplied ? AppTheme.StatusGreen : AppTheme.StatusAmber,
                ItemTag = setting,
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

    public int LoadSettings()
    {
        var count = LoadSettingsData();
        PopulateUI();
        return count;
    }

    public List<NetworkSetting> GetUnappliedSettings() =>
        _settings.Where(s => !s.IsApplied).ToList();

    public (int applied, List<string> errors) ApplyAll()
    {
        var unapplied = GetUnappliedSettings();
        if (unapplied.Count == 0)
            return (0, new List<string>());

        var result = _optimizer.ApplySettings(unapplied);
        LoadSettings();
        return result;
    }

    private async void BtnApply_Click(object? sender, EventArgs e)
    {
        var selectedSettings = new List<NetworkSetting>();
        foreach (var ctrl in itemsPanel.Controls.OfType<ModernCheckItem>())
        {
            if (ctrl.IsChecked && ctrl.ItemTag is NetworkSetting s && !s.IsApplied)
                selectedSettings.Add(s);
        }

        if (selectedSettings.Count == 0)
        {
            MessageBox.Show("No new network optimizations selected to apply.", "WinOptimizer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var names = string.Join("\n", selectedSettings.Select(s => $"  - {s.Name}: {s.Description}"));
        var confirm = MessageBox.Show(
            $"The following network registry values will be modified:\n\n{names}\n\nContinue?",
            "Confirm Network Optimization",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        if (!RestorePointService.PromptAndCreate("WinOptimizer - Before network optimization"))
            return;

        btnApply.Enabled = false;
        btnRevert.Enabled = false;
        _mainForm.SetStatus("Applying network optimizations...");
        _mainForm.SetProgress(30);

        var (applied, errors) = await Task.Run(() => _optimizer.ApplySettings(selectedSettings));

        _mainForm.SetProgress(90);
        var msg = $"Applied {applied} network optimization(s).";
        if (errors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", errors.Select(err => $"  - {err}"));

        if (applied > 0)
            msg += "\n\nNote: A system reboot may be required for network changes to take effect.";

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        await Task.Run(() => LoadSettingsData());
        PopulateUI();
        _mainForm.SetProgress(100);
        btnApply.Enabled = true;
        btnRevert.Enabled = true;
    }

    private async void BtnRevert_Click(object? sender, EventArgs e)
    {
        var selectedSettings = new List<NetworkSetting>();
        foreach (var ctrl in itemsPanel.Controls.OfType<ModernCheckItem>())
        {
            if (ctrl.IsChecked && ctrl.ItemTag is NetworkSetting s && s.IsApplied)
                selectedSettings.Add(s);
        }

        if (selectedSettings.Count == 0)
        {
            MessageBox.Show("No applied network optimizations selected to revert.", "WinOptimizer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Revert {selectedSettings.Count} network optimization(s) to defaults?\n\nA reboot may be required.",
            "Confirm Revert",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        btnApply.Enabled = false;
        btnRevert.Enabled = false;
        _mainForm.SetStatus("Reverting network optimizations...");
        _mainForm.SetProgress(30);

        var (reverted, errors) = await Task.Run(() => _optimizer.RevertSettings(selectedSettings));

        _mainForm.SetProgress(90);
        var msg = $"Reverted {reverted} network optimization(s).";
        if (errors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", errors.Select(err => $"  - {err}"));

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        await Task.Run(() => LoadSettingsData());
        PopulateUI();
        _mainForm.SetProgress(100);
        btnApply.Enabled = true;
        btnRevert.Enabled = true;
    }
}
