namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Theme;

partial class NetworkOptimizationControl
{
    private System.ComponentModel.IContainer components = null;
    private ModernButton btnApply;
    private ModernButton btnRevert;
    private Panel itemsPanel;
    private Label lblTitle;
    private Label lblSubtitle;
    private Label lblNote;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var p = AppTheme.ContentPadding;

        // Header panel (docked top)
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 130,
            BackColor = Color.White,
        };

        lblTitle = new Label
        {
            Text = "Network Optimization",
            Font = AppTheme.PageTitleFont,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(p, 12),
            AutoSize = true,
        };

        lblSubtitle = new Label
        {
            Text = "Optimize Ethernet and TCP settings for better network performance",
            Font = AppTheme.PageSubtitleFont,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(p, 52),
            AutoSize = true,
        };

        var btnY = 78;
        btnApply = new ModernButton { Text = "Apply Selected", Location = new Point(p, btnY), Size = new Size(130, 34) };
        btnApply.Click += BtnApply_Click;

        btnRevert = new ModernButton { Text = "Revert Selected", IsPrimary = false, Location = new Point(p + 140, btnY), Size = new Size(140, 34) };
        btnRevert.Click += BtnRevert_Click;

        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblSubtitle);
        headerPanel.Controls.Add(btnApply);
        headerPanel.Controls.Add(btnRevert);

        // Footer panel (docked bottom)
        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.White,
        };

        lblNote = new Label
        {
            Text = "Note: A system reboot may be required for network changes to take effect.",
            Font = AppTheme.CardSecondaryFont,
            ForeColor = AppTheme.StatusRed,
            Location = new Point(p, 4),
            AutoSize = true,
        };

        footerPanel.Controls.Add(lblNote);

        // Items panel (fills remaining space)
        itemsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding = new Padding(p, 0, p, 0),
        };

        BackColor = Color.White;
        Controls.Add(itemsPanel);
        Controls.Add(footerPanel);
        Controls.Add(headerPanel);
    }
}
