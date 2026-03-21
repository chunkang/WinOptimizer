using WinOptimizer.Controls;

namespace WinOptimizer.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabControl;
    private TabPage tabSoftware;
    private TabPage tabSystem;
    private TabPage tabNetwork;
    private TabPage tabBrowserCache;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        tabControl = new TabControl();
        tabSoftware = new TabPage();
        tabSystem = new TabPage();
        tabNetwork = new TabPage();
        tabBrowserCache = new TabPage();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();

        // tabControl
        tabControl.Dock = DockStyle.Fill;
        tabControl.TabPages.Add(tabSoftware);
        tabControl.TabPages.Add(tabSystem);
        tabControl.TabPages.Add(tabNetwork);
        tabControl.TabPages.Add(tabBrowserCache);

        // tabSoftware
        tabSoftware.Text = "Security Software";
        tabSoftware.Padding = new Padding(8);
        var softwareControl = new SoftwareDetectionControl(this);
        softwareControl.Dock = DockStyle.Fill;
        tabSoftware.Controls.Add(softwareControl);

        // tabSystem
        tabSystem.Text = "System Optimization";
        tabSystem.Padding = new Padding(8);
        var systemControl = new SystemOptimizationControl(this);
        systemControl.Dock = DockStyle.Fill;
        tabSystem.Controls.Add(systemControl);

        // tabNetwork
        tabNetwork.Text = "Network Optimization";
        tabNetwork.Padding = new Padding(8);
        var networkControl = new NetworkOptimizationControl(this);
        networkControl.Dock = DockStyle.Fill;
        tabNetwork.Controls.Add(networkControl);

        // tabBrowserCache
        tabBrowserCache.Text = "Browser Cache";
        tabBrowserCache.Padding = new Padding(8);
        var browserCacheControl = new BrowserCacheCleanupControl(this);
        browserCacheControl.Dock = DockStyle.Fill;
        tabBrowserCache.Controls.Add(browserCacheControl);

        // statusStrip
        statusLabel.Spring = true;
        statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        statusLabel.Text = "Ready";
        progressBar.Visible = false;
        progressBar.Size = new Size(150, 16);
        statusStrip.Items.Add(statusLabel);
        statusStrip.Items.Add(progressBar);

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(700, 520);
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(700, 520);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WinOptimizer";

        Controls.Add(tabControl);
        Controls.Add(statusStrip);
    }
}
