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
                var plusIndex = infoVer.IndexOf('+');
                return plusIndex >= 0 ? infoVer[..plusIndex] : infoVer;
            }
            var ver = asm.GetName().Version;
            return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
        }
    }

    public MainForm()
    {
        InitializeComponent();
        Text = $"WinOptimizer v{AppVersion}";
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
