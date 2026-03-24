using WinOptimizer.Controls;
using WinOptimizer.Controls.Modern;

namespace WinOptimizer.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private ModernSidebar sidebar;
    private ModernStatusBar statusBar;
    private Panel contentPanel;
    private DashboardControl dashboardControl;
    private SoftwareDetectionControl softwareControl;
    private SystemOptimizationControl systemControl;
    private NetworkOptimizationControl networkControl;
    private BrowserCacheCleanupControl browserCacheControl;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        sidebar = new ModernSidebar();
        statusBar = new ModernStatusBar();
        contentPanel = new Panel();

        SuspendLayout();

        // sidebar
        sidebar.Dock = DockStyle.Left;
        sidebar.AddItem(new ModernSidebar.NavItem("Dashboard", "\u25A3"));
        sidebar.AddItem(new ModernSidebar.NavItem("Security", "\u26A0"));
        sidebar.AddItem(new ModernSidebar.NavItem("System", "\u2699"));
        sidebar.AddItem(new ModernSidebar.NavItem("Network", "\u26A1"));
        sidebar.AddItem(new ModernSidebar.NavItem("Browser", "\u2601"));
        sidebar.SelectedIndexChanged += Sidebar_SelectedIndexChanged;
        sidebar.BtnScanAll.Click += BtnScanAll_Click;
        sidebar.BtnFixAll.Click += BtnFixAll_Click;

        // contentPanel
        contentPanel.Dock = DockStyle.Fill;
        contentPanel.BackColor = Color.White;
        contentPanel.Padding = new Padding(0);

        // page controls
        dashboardControl = new DashboardControl { Dock = DockStyle.Fill };
        dashboardControl.NavigateToPage += (_, idx) => sidebar.SelectedIndex = idx;

        softwareControl = new SoftwareDetectionControl(this) { Dock = DockStyle.Fill, Visible = false };
        systemControl = new SystemOptimizationControl(this) { Dock = DockStyle.Fill, Visible = false };
        networkControl = new NetworkOptimizationControl(this) { Dock = DockStyle.Fill, Visible = false };
        browserCacheControl = new BrowserCacheCleanupControl(this) { Dock = DockStyle.Fill, Visible = false };

        contentPanel.Controls.Add(dashboardControl);
        contentPanel.Controls.Add(softwareControl);
        contentPanel.Controls.Add(systemControl);
        contentPanel.Controls.Add(networkControl);
        contentPanel.Controls.Add(browserCacheControl);

        // statusBar
        statusBar.Dock = DockStyle.Bottom;

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 620);
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WinOptimizer";

        Controls.Add(contentPanel);
        Controls.Add(sidebar);
        Controls.Add(statusBar);

        ResumeLayout(false);
        PerformLayout();
    }
}
