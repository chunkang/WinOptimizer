namespace WinOptimizer.Forms;

using WinOptimizer.Controls;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
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
