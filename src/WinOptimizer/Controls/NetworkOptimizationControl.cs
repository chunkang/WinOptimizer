namespace WinOptimizer.Controls;

using WinOptimizer.Forms;
using WinOptimizer.Models;
using WinOptimizer.Services;

public partial class NetworkOptimizationControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly NetworkOptimizerService _optimizer = new();
    private List<NetworkSetting> _settings = new();

    public NetworkOptimizationControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeComponent();
        LoadSettings();
    }

    public int LoadSettings()
    {
        checkedListBox.Items.Clear();
        _settings = _optimizer.GetSettings();

        var unappliedCount = 0;
        foreach (var setting in _settings)
        {
            var label = setting.IsApplied
                ? $"{setting.Name} (Applied)"
                : setting.Name;
            var index = checkedListBox.Items.Add(label);
            checkedListBox.SetItemChecked(index, !setting.IsApplied);
            if (!setting.IsApplied) unappliedCount++;
        }

        return unappliedCount;
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

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        var selectedSettings = new List<NetworkSetting>();
        for (int i = 0; i < checkedListBox.Items.Count; i++)
        {
            if (checkedListBox.GetItemChecked(i) && !_settings[i].IsApplied)
                selectedSettings.Add(_settings[i]);
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

        _mainForm.SetStatus("Applying network optimizations...");
        var (applied, errors) = _optimizer.ApplySettings(selectedSettings);

        var msg = $"Applied {applied} network optimization(s).";
        if (errors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));

        if (applied > 0)
            msg += "\n\nNote: A system reboot may be required for network changes to take effect.";

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        LoadSettings();
    }

    private void BtnRevert_Click(object? sender, EventArgs e)
    {
        var selectedSettings = new List<NetworkSetting>();
        for (int i = 0; i < checkedListBox.Items.Count; i++)
        {
            if (checkedListBox.GetItemChecked(i) && _settings[i].IsApplied)
                selectedSettings.Add(_settings[i]);
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

        _mainForm.SetStatus("Reverting network optimizations...");
        var (reverted, errors) = _optimizer.RevertSettings(selectedSettings);

        var msg = $"Reverted {reverted} network optimization(s).";
        if (errors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        LoadSettings();
    }
}
