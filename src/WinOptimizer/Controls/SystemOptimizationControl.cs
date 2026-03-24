namespace WinOptimizer.Controls;

using WinOptimizer.Forms;
using WinOptimizer.Models;
using WinOptimizer.Services;

public partial class SystemOptimizationControl : UserControl
{
    private readonly MainForm _mainForm;
    private readonly RegistryOptimizerService _optimizer = new();
    private readonly SystemCleanupService _cleanup = new();
    private List<OptimizationSetting> _settings = new();
    private List<CleanupTask> _cleanupTasks = new();

    public SystemOptimizationControl(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeComponent();
        LoadSettings();
    }

    public int LoadSettings()
    {
        checkedListBox.Items.Clear();
        _settings = _optimizer.GetSettings();
        _cleanupTasks = _cleanup.Scan();

        var actionableCount = 0;

        // Registry optimizations
        foreach (var setting in _settings)
        {
            var label = setting.IsApplied
                ? $"{setting.Name} (Applied)"
                : setting.Name;
            var index = checkedListBox.Items.Add(label);
            checkedListBox.SetItemChecked(index, !setting.IsApplied);
            if (!setting.IsApplied) actionableCount++;
        }

        // Cleanup tasks
        foreach (var task in _cleanupTasks)
        {
            var sizeLabel = task.IsCleanable
                ? SystemCleanupService.FormatBytes(task.SizeBytes)
                : "Empty";
            var label = $"{task.Name} ({sizeLabel})";
            var index = checkedListBox.Items.Add(label);
            checkedListBox.SetItemChecked(index, task.IsCleanable);
            if (task.IsCleanable) actionableCount++;
        }

        return actionableCount;
    }

    public List<OptimizationSetting> GetUnappliedSettings() =>
        _settings.Where(s => !s.IsApplied).ToList();

    public List<CleanupTask> GetCleanableTasks() =>
        _cleanupTasks.Where(t => t.IsCleanable).ToList();

    public (int applied, List<string> errors) ApplyAll()
    {
        var totalApplied = 0;
        var allErrors = new List<string>();

        // Apply registry optimizations
        var unapplied = GetUnappliedSettings();
        if (unapplied.Count > 0)
        {
            var (applied, errors) = _optimizer.ApplySettings(unapplied);
            totalApplied += applied;
            allErrors.AddRange(errors);
        }

        // Run cleanup tasks
        var cleanable = GetCleanableTasks();
        if (cleanable.Count > 0)
        {
            var (cleaned, _, errors) = _cleanup.Clean(cleanable);
            totalApplied += cleaned;
            allErrors.AddRange(errors);
        }

        LoadSettings();
        return (totalApplied, allErrors);
    }

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        var selectedSettings = new List<OptimizationSetting>();
        var selectedCleanup = new List<CleanupTask>();

        for (int i = 0; i < checkedListBox.Items.Count; i++)
        {
            if (!checkedListBox.GetItemChecked(i)) continue;

            if (i < _settings.Count)
            {
                if (!_settings[i].IsApplied)
                    selectedSettings.Add(_settings[i]);
            }
            else
            {
                var cleanupIndex = i - _settings.Count;
                if (_cleanupTasks[cleanupIndex].IsCleanable)
                    selectedCleanup.Add(_cleanupTasks[cleanupIndex]);
            }
        }

        if (selectedSettings.Count == 0 && selectedCleanup.Count == 0)
        {
            MessageBox.Show("No actions selected to apply.", "WinOptimizer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Build confirmation message
        var lines = new List<string>();
        lines.AddRange(selectedSettings.Select(s => $"  - {s.Name}: {s.Description}"));
        lines.AddRange(selectedCleanup.Select(t =>
            $"  - {t.Name}: {t.Description} ({SystemCleanupService.FormatBytes(t.SizeBytes)})"));

        var confirm = MessageBox.Show(
            $"The following actions will be performed:\n\n{string.Join("\n", lines)}\n\nContinue?",
            "Confirm Optimization",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        if (selectedSettings.Count > 0)
        {
            if (!RestorePointService.PromptAndCreate("WinOptimizer - Before system optimization"))
                return;
        }

        _mainForm.SetStatus("Applying system optimizations...");
        var totalApplied = 0;
        var allErrors = new List<string>();

        if (selectedSettings.Count > 0)
        {
            var (applied, errors) = _optimizer.ApplySettings(selectedSettings);
            totalApplied += applied;
            allErrors.AddRange(errors);
        }

        if (selectedCleanup.Count > 0)
        {
            _mainForm.SetStatus("Cleaning up...");
            var (cleaned, freedBytes, errors) = _cleanup.Clean(selectedCleanup);
            totalApplied += cleaned;
            allErrors.AddRange(errors);
        }

        var msg = $"Completed {totalApplied} action(s).";
        if (allErrors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", allErrors.Select(e => $"  - {e}"));

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, allErrors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        LoadSettings();
    }

    private void BtnRevert_Click(object? sender, EventArgs e)
    {
        var selectedSettings = new List<OptimizationSetting>();
        for (int i = 0; i < _settings.Count; i++)
        {
            if (checkedListBox.GetItemChecked(i) && _settings[i].IsApplied)
                selectedSettings.Add(_settings[i]);
        }

        if (selectedSettings.Count == 0)
        {
            MessageBox.Show("No applied optimizations selected to revert.", "WinOptimizer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Revert {selectedSettings.Count} optimization(s) to their default values?",
            "Confirm Revert",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        _mainForm.SetStatus("Reverting optimizations...");
        var (reverted, errors) = _optimizer.RevertSettings(selectedSettings);

        var msg = $"Reverted {reverted} optimization(s).";
        if (errors.Count > 0)
            msg += "\n\nErrors:\n" + string.Join("\n", errors.Select(e => $"  - {e}"));

        MessageBox.Show(msg, "Results",
            MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        _mainForm.SetStatus(msg.Split('\n')[0]);
        LoadSettings();
    }
}
