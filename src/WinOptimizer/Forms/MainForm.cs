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
                var plusIndex = infoVer.IndexOf("+build.");
                if (plusIndex >= 0)
                    return infoVer[..plusIndex];
                return infoVer;
            }
            var ver = asm.GetName().Version;
            return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
        }
    }

    private readonly Control[] _pages;

    public MainForm()
    {
        InitializeComponent();
        Text = $"WinOptimizer v{AppVersion}";
        var exePath = Environment.ProcessPath;
        if (exePath != null)
            Icon = Icon.ExtractAssociatedIcon(exePath);
        sidebar.AppVersion = AppVersion;

        _pages = new Control[] { dashboardControl, softwareControl, systemControl, networkControl, browserCacheControl };

        Shown += async (_, _) => await ScanAllAsync();
    }

    private void Sidebar_SelectedIndexChanged(object? sender, int index)
    {
        for (int i = 0; i < _pages.Length; i++)
            _pages[i].Visible = i == index;
    }

    // Maps sidebar nav indices (1-4) to badge indices
    public void UpdateTabBadge(int tabIndex, string? badge)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateTabBadge(tabIndex, badge));
            return;
        }
        // tabIndex 0-3 maps to sidebar nav items 1-4 (0 is Dashboard)
        sidebar.SetBadge(tabIndex + 1, badge);
    }

    private async Task ScanAllAsync()
    {
        sidebar.BtnScanAll.Enabled = false;
        SetStatus("Scanning all...");
        SetProgress(25);

        // Security Software (async)
        var swCount = await softwareControl.ScanAsync();
        UpdateTabBadge(0, swCount > 0 ? $"{swCount} found" : null);
        dashboardControl.UpdateSecurityCard(swCount);

        SetProgress(50);

        // System Optimization
        var sysUnapplied = systemControl.LoadSettings();
        UpdateTabBadge(1, sysUnapplied > 0 ? $"{sysUnapplied} available" : null);
        dashboardControl.UpdateSystemCard(sysUnapplied);

        SetProgress(75);

        // Network Optimization
        var netUnapplied = networkControl.LoadSettings();
        UpdateTabBadge(2, netUnapplied > 0 ? $"{netUnapplied} available" : null);
        dashboardControl.UpdateNetworkCard(netUnapplied);

        // Browser Cache
        var (_, totalBytes) = browserCacheControl.ScanBrowsers();
        UpdateTabBadge(3, totalBytes > 0
            ? BrowserCacheCleanupService.FormatBytes(totalBytes)
            : null);
        dashboardControl.UpdateBrowserCard(totalBytes);

        SetProgress(100);
        SetStatus("Scan complete.");
        dashboardControl.SetLastScanTime();
        sidebar.BtnScanAll.Enabled = true;

        // Enable Fix All if there's anything actionable
        var hasWork = swCount > 0 || sysUnapplied > 0 || netUnapplied > 0 || totalBytes > 0;
        sidebar.BtnFixAll.Enabled = hasWork;
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

        sidebar.BtnScanAll.Enabled = false;
        sidebar.BtnFixAll.Enabled = false;
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
        statusBar.StatusText = message;
    }

    public void SetProgress(int value)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetProgress(value));
            return;
        }
        statusBar.ProgressValue = Math.Clamp(value, 0, 100);
    }
}
