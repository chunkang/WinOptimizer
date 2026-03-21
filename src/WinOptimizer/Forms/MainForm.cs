namespace WinOptimizer.Forms;

using System.Reflection;
using WinOptimizer.Controls;

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

    public MainForm()
    {
        InitializeComponent();
        Text = $"WinOptimizer v{AppVersion}";
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
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
