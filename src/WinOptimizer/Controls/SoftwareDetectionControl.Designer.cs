// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Theme;

partial class SoftwareDetectionControl
{
    private System.ComponentModel.IContainer components = null;
    private ModernButton btnScan;
    private ModernButton btnSelectAll;
    private ModernButton btnDeselectAll;
    private ModernButton btnUninstall;
    private Panel itemsPanel;
    private Label lblTitle;
    private Label lblSubtitle;
    private Label lblCount;

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
            Text = "Security Software",
            Font = AppTheme.PageTitleFont,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(p, 12),
            AutoSize = true,
        };

        lblSubtitle = new Label
        {
            Text = "Detect and remove unnecessary Korean banking/security software",
            Font = AppTheme.PageSubtitleFont,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(p, 52),
            AutoSize = true,
        };

        var btnY = 78;
        btnScan = new ModernButton { Text = "Scan Now", Location = new Point(p, btnY), Size = new Size(110, 34) };
        btnScan.Click += BtnScan_Click;

        btnSelectAll = new ModernButton { Text = "Select All", IsPrimary = false, Location = new Point(p + 120, btnY), Size = new Size(100, 34) };
        btnSelectAll.Click += BtnSelectAll_Click;

        btnDeselectAll = new ModernButton { Text = "Deselect All", IsPrimary = false, Location = new Point(p + 230, btnY), Size = new Size(110, 34) };
        btnDeselectAll.Click += BtnDeselectAll_Click;

        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblSubtitle);
        headerPanel.Controls.Add(btnScan);
        headerPanel.Controls.Add(btnSelectAll);
        headerPanel.Controls.Add(btnDeselectAll);

        // Footer panel (docked bottom)
        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            BackColor = Color.White,
        };

        lblCount = new Label
        {
            Text = "Click 'Scan Now' to detect banking/security software",
            Font = AppTheme.CardSecondaryFont,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(p, 8),
            AutoSize = true,
        };

        btnUninstall = new ModernButton
        {
            Text = "Uninstall Selected",
            Size = new Size(160, 36),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        btnUninstall.Location = new Point(footerPanel.Width - btnUninstall.Width - p, 4);
        btnUninstall.Click += BtnUninstall_Click;

        footerPanel.Controls.Add(lblCount);
        footerPanel.Controls.Add(btnUninstall);

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
