namespace WinOptimizer;

using WinOptimizer.Helpers;
using WinOptimizer.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        if (!AdminHelper.IsRunningAsAdmin())
        {
            AdminHelper.RestartAsAdmin();
            return;
        }

        LogHelper.Initialize();
        LogHelper.Log("WinOptimizer started");

        Application.Run(new MainForm());
    }
}
