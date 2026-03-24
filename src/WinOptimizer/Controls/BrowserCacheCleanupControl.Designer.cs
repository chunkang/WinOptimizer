// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Theme;

partial class BrowserCacheCleanupControl
{
    private System.ComponentModel.IContainer components = null;
    private ModernButton btnScan;
    private ModernButton btnClean;
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
            Text = "Browser Cache",
            Font = AppTheme.PageTitleFont,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(p, 12),
            AutoSize = true,
        };

        lblSubtitle = new Label
        {
            Text = "Clean browser caches to free disk space. Bookmarks and passwords are not affected.",
            Font = AppTheme.PageSubtitleFont,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(p, 52),
            AutoSize = true,
        };

        var btnY = 78;
        btnScan = new ModernButton { Text = "Rescan", IsPrimary = false, Location = new Point(p, btnY), Size = new Size(100, 34) };
        btnScan.Click += BtnScan_Click;

        btnClean = new ModernButton { Text = "Clean Selected", Location = new Point(p + 110, btnY), Size = new Size(140, 34) };
        btnClean.Click += BtnClean_Click;

        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblSubtitle);
        headerPanel.Controls.Add(btnScan);
        headerPanel.Controls.Add(btnClean);

        // Footer panel (docked bottom)
        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.White,
        };

        lblNote = new Label
        {
            Text = "Tip: Close all browsers before cleaning for best results.",
            Font = AppTheme.CardSecondaryFont,
            ForeColor = AppTheme.Accent,
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
