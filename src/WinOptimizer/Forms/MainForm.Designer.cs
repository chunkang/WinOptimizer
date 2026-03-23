using WinOptimizer.Controls;

namespace WinOptimizer.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel bottomButtonPanel;
    private Button btnScanAll;
    private Button btnFixAll;
    private TabControl tabControl;
    private TabPage tabSoftware;
    private TabPage tabSystem;
    private TabPage tabNetwork;
    private TabPage tabBrowserCache;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
    private SoftwareDetectionControl softwareControl;
    private SystemOptimizationControl systemControl;
    private NetworkOptimizationControl networkControl;
    private BrowserCacheCleanupControl browserCacheControl;

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

        bottomButtonPanel = new Panel();
        btnScanAll = new Button();
        btnFixAll = new Button();
        tabControl = new TabControl();
        tabSoftware = new TabPage();
        tabSystem = new TabPage();
        tabNetwork = new TabPage();
        tabBrowserCache = new TabPage();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();

        // bottomButtonPanel
        bottomButtonPanel.Dock = DockStyle.Bottom;
        bottomButtonPanel.Height = 45;
        bottomButtonPanel.Padding = new Padding(8, 4, 8, 8);

        // btnScanAll
        btnScanAll.Text = "Scan All";
        btnScanAll.Size = new Size(100, 30);
        btnScanAll.Location = new Point(8, 8);
        btnScanAll.Click += BtnScanAll_Click;
        bottomButtonPanel.Controls.Add(btnScanAll);

        // btnFixAll
        btnFixAll.Text = "Fix All";
        btnFixAll.Size = new Size(100, 30);
        btnFixAll.Location = new Point(116, 8);
        btnFixAll.Enabled = false;
        btnFixAll.Click += BtnFixAll_Click;
        bottomButtonPanel.Controls.Add(btnFixAll);

        // tabControl
        tabControl.Dock = DockStyle.Fill;
        tabControl.TabPages.Add(tabSoftware);
        tabControl.TabPages.Add(tabSystem);
        tabControl.TabPages.Add(tabNetwork);
        tabControl.TabPages.Add(tabBrowserCache);

        // tabSoftware
        tabSoftware.Text = "Security Software";
        tabSoftware.Padding = new Padding(8);
        softwareControl = new SoftwareDetectionControl(this);
        softwareControl.Dock = DockStyle.Fill;
        tabSoftware.Controls.Add(softwareControl);

        // tabSystem
        tabSystem.Text = "System Optimization";
        tabSystem.Padding = new Padding(8);
        systemControl = new SystemOptimizationControl(this);
        systemControl.Dock = DockStyle.Fill;
        tabSystem.Controls.Add(systemControl);

        // tabNetwork
        tabNetwork.Text = "Network Optimization";
        tabNetwork.Padding = new Padding(8);
        networkControl = new NetworkOptimizationControl(this);
        networkControl.Dock = DockStyle.Fill;
        tabNetwork.Controls.Add(networkControl);

        // tabBrowserCache
        tabBrowserCache.Text = "Browser Cache";
        tabBrowserCache.Padding = new Padding(8);
        browserCacheControl = new BrowserCacheCleanupControl(this);
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
        Controls.Add(bottomButtonPanel);
        Controls.Add(statusStrip);
    }
}
