namespace WinOptimizer.Forms;

using System.Reflection;
using System.Text;
using WinOptimizer.Controls;
using WinOptimizer.Services;

public partial class MainForm : Form
{
    public static string AppVersion
    {
        get
        {
            var asm = Assembly.GetExecutingAssembly();
            var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (info != null)
            {
                var infoVer = info.InformationalVersion;
                // Show build number: "0.1.4+build.2" → "0.1.4 (build 2)"
                var plusIndex = infoVer.IndexOf("+build.");
                if (plusIndex >= 0)
                {
                    var semver = infoVer[..plusIndex];
                    var buildNum = infoVer[(plusIndex + 7)..];
                    return $"{semver} (build {buildNum})";
                }
                return infoVer;
            }
            var ver = asm.GetName().Version;
            return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
        }
    }

    private static readonly string[] TabBaseNames = { "Security Software", "System Optimization", "Network Optimization", "Browser Cache" };

    public MainForm()
    {
        InitializeComponent();
        Text = $"WinOptimizer v{AppVersion}";
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        Shown += async (_, _) => await ScanAllAsync();
    }

    public void UpdateTabBadge(int tabIndex, string? badge)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateTabBadge(tabIndex, badge));
            return;
        }
        var tab = tabControl.TabPages[tabIndex];
        tab.Text = badge != null
            ? $"{TabBaseNames[tabIndex]} ({badge})"
            : TabBaseNames[tabIndex];
    }

    private async Task ScanAllAsync()
    {
        btnScanAll.Enabled = false;
        SetStatus("Scanning all...");
        SetProgress(25);

        // Security Software (async)
        var swCount = await softwareControl.ScanAsync();
        UpdateTabBadge(0, swCount > 0 ? $"{swCount} found" : null);

        SetProgress(50);

        // System Optimization
        var sysUnapplied = systemControl.LoadSettings();
        UpdateTabBadge(1, sysUnapplied > 0 ? $"{sysUnapplied} available" : null);

        SetProgress(75);

        // Network Optimization
        var netUnapplied = networkControl.LoadSettings();
        UpdateTabBadge(2, netUnapplied > 0 ? $"{netUnapplied} available" : null);

        // Browser Cache
        var (_, totalBytes) = browserCacheControl.ScanBrowsers();
        UpdateTabBadge(3, totalBytes > 0
            ? Services.BrowserCacheCleanupService.FormatBytes(totalBytes)
            : null);

        SetProgress(100);
        SetStatus("Scan complete.");
        btnScanAll.Enabled = true;

        // Enable Fix All if there's anything actionable
        var hasWork = swCount > 0 || sysUnapplied > 0 || netUnapplied > 0 || totalBytes > 0;
        btnFixAll.Enabled = hasWork;
    }

    private async void BtnScanAll_Click(object? sender, EventArgs e)
    {
        await ScanAllAsync();
    }

    private async void BtnFixAll_Click(object? sender, EventArgs e)
    {
        // Build summary of what will be done
        var summary = new StringBuilder();
        var swList = softwareControl.GetDetectedSoftware();
        var sysUnapplied = systemControl.GetUnappliedSettings();
        var netUnapplied = networkControl.GetUnappliedSettings();
        var cleanable = browserCacheControl.GetCleanableBrowsers();

        var cleanupTasks = systemControl.GetCleanableTasks();

        if (swList.Count > 0)
            summary.AppendLine($"Uninstall {swList.Count} security software");
        if (sysUnapplied.Count > 0)
            summary.AppendLine($"Apply {sysUnapplied.Count} system optimization(s)");
        if (cleanupTasks.Count > 0)
        {
            var cleanupSize = cleanupTasks.Sum(t => t.SizeBytes);
            summary.AppendLine($"Clean {cleanupTasks.Count} item(s) ({SystemCleanupService.FormatBytes(cleanupSize)})");
        }
        if (netUnapplied.Count > 0)
            summary.AppendLine($"Apply {netUnapplied.Count} network optimization(s)");
        if (cleanable.Count > 0)
        {
            var totalSize = cleanable.Sum(b => b.CacheSizeBytes);
            summary.AppendLine($"Clean {cleanable.Count} browser cache(s) ({BrowserCacheCleanupService.FormatBytes(totalSize)})");
        }

        if (summary.Length == 0)
        {
            MessageBox.Show("Nothing to fix.", "WinOptimizer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"The following actions will be performed:\n\n{summary}\nContinue?",
            "Confirm Fix All",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        if (swList.Count > 0 || sysUnapplied.Count > 0 || netUnapplied.Count > 0)
        {
            if (!RestorePointService.PromptAndCreate("WinOptimizer - Before Fix All"))
                return;
        }

        btnScanAll.Enabled = false;
        btnFixAll.Enabled = false;
        var results = new StringBuilder();

        // 1. Uninstall security software
        if (swList.Count > 0)
        {
            SetStatus("Uninstalling security software...");
            SetProgress(20);
            var progress = new Progress<string>(msg => SetStatus(msg));
            var (succeeded, failed, errors) = await softwareControl.UninstallAllAsync(progress);
            results.AppendLine($"Software: {succeeded} uninstalled, {failed} failed");
            foreach (var err in errors) results.AppendLine($"  - {err}");
        }

        // 2. Apply system optimizations and cleanup
        if (sysUnapplied.Count > 0 || cleanupTasks.Count > 0)
        {
            SetStatus("Applying system optimizations...");
            SetProgress(40);
            var (applied, errors) = systemControl.ApplyAll();
            results.AppendLine($"System: {applied} action(s) applied");
            foreach (var err in errors) results.AppendLine($"  - {err}");
        }

        // 3. Apply network optimizations
        if (netUnapplied.Count > 0)
        {
            SetStatus("Applying network optimizations...");
            SetProgress(60);
            var (applied, errors) = networkControl.ApplyAll();
            results.AppendLine($"Network: {applied} optimization(s) applied");
            foreach (var err in errors) results.AppendLine($"  - {err}");
        }

        // 4. Clean browser caches
        if (cleanable.Count > 0)
        {
            SetStatus("Cleaning browser caches...");
            SetProgress(80);
            var (cleaned, freedBytes, errors) = browserCacheControl.CleanAll();
            results.AppendLine($"Browser: {cleaned} cache(s) cleaned, freed {BrowserCacheCleanupService.FormatBytes(freedBytes)}");
            foreach (var err in errors) results.AppendLine($"  - {err}");
        }

        SetProgress(100);
        SetStatus("Fix All complete.");

        MessageBox.Show(results.ToString().TrimEnd(), "Fix All Results",
            MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Re-scan to refresh everything
        await ScanAllAsync();
    }

    public void SetStatus(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetStatus(message));
            return;
        }
        statusLabel.Text = message;
    }

    public void SetProgress(int value)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetProgress(value));
            return;
        }
        progressBar.Value = Math.Clamp(value, 0, 100);
        progressBar.Visible = value > 0 && value < 100;
    }
}
