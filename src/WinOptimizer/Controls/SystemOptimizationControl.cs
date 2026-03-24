namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Forms;
using WinOptimizer.Models;
using WinOptimizer.Services;
using WinOptimizer.Theme;

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
    }

    public int LoadSettings()
    {
        itemsPanel.SuspendLayout();
        itemsPanel.Controls.Clear();
        _settings = _optimizer.GetSettings();
        _cleanupTasks = _cleanup.Scan();

        var y = 0;
        var actionableCount = 0;

        // Group registry settings by category
        var groups = _settings.GroupBy(s => s.Category);
        foreach (var group in groups)
        {
            // Category header label
            var header = new Label
            {
                Text = group.Key,
                Font = AppTheme.CardTitleFont,
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(0, y),
                Size = new Size(itemsPanel.ClientSize.Width, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            itemsPanel.Controls.Add(header);
            y += 30;

            foreach (var setting in group)
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
                if (!setting.IsApplied) actionableCount++;
            }

            y += 8; // gap between groups
        }

        // Cleanup tasks section
        if (_cleanupTasks.Count > 0)
        {
            var cleanupHeader = new Label
            {
                Text = "Disk Cleanup",
                Font = AppTheme.CardTitleFont,
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(0, y),
                Size = new Size(itemsPanel.ClientSize.Width, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            itemsPanel.Controls.Add(cleanupHeader);
            y += 30;

            foreach (var task in _cleanupTasks)
            {
                var sizeLabel = task.IsCleanable
                    ? SystemCleanupService.FormatBytes(task.SizeBytes)
                    : "Empty";
                var item = new ModernCheckItem
                {
                    Text = task.Name,
                    Description = task.Description,
                    IsChecked = task.IsCleanable,
                    StatusText = sizeLabel,
                    StatusColor = task.IsCleanable ? AppTheme.StatusAmber : AppTheme.StatusGreen,
                    ItemTag = task,
                    Height = 54,
                    Location = new Point(0, y),
                    Width = itemsPanel.ClientSize.Width,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                };
                itemsPanel.Controls.Add(item);
                y += item.Height;
                if (task.IsCleanable) actionableCount++;
            }
        }

        itemsPanel.ResumeLayout();
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

        var unapplied = GetUnappliedSettings();
        if (unapplied.Count > 0)
        {
            var (applied, errors) = _optimizer.ApplySettings(unapplied);
            totalApplied += applied;
            allErrors.AddRange(errors);
        }

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

        foreach (var ctrl in itemsPanel.Controls.OfType<ModernCheckItem>())
        {
            if (!ctrl.IsChecked) continue;
            if (ctrl.ItemTag is OptimizationSetting s && !s.IsApplied)
                selectedSettings.Add(s);
            else if (ctrl.ItemTag is CleanupTask t && t.IsCleanable)
                selectedCleanup.Add(t);
        }

        if (selectedSettings.Count == 0 && selectedCleanup.Count == 0)
        {
            MessageBox.Show("No actions selected to apply.", "WinOptimizer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

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
        foreach (var ctrl in itemsPanel.Controls.OfType<ModernCheckItem>())
        {
            if (ctrl.IsChecked && ctrl.ItemTag is OptimizationSetting s && s.IsApplied)
                selectedSettings.Add(s);
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
