// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Johnny Kang <abjohnkang@gmail.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public enum SummaryStatus
{
    NotScanned,
    Clear,
    ActionNeeded,
}

public class DashboardSummaryCard : Control
{
    private static readonly Font _iconFont = new("Segoe UI", 15f);
    private string _icon = "";
    private string _title = "";
    private string _detail = "Not scanned";
    private SummaryStatus _status = SummaryStatus.NotScanned;
    private bool _isHovered;

    public event EventHandler? CardClicked;

    public string Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    public string Title
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    public string Detail
    {
        get => _detail;
        set { _detail = value; Invalidate(); }
    }

    public SummaryStatus Status
    {
        get => _status;
        set { _status = value; Invalidate(); }
    }

    public DashboardSummaryCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(280, 120);
        Cursor = Cursors.Hand;
    }

    private Color StatusAccent => _status switch
    {
        SummaryStatus.Clear => AppTheme.StatusGreen,
        SummaryStatus.ActionNeeded => AppTheme.StatusAmber,
        _ => AppTheme.Disabled,
    };

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        var cardRect = new RectangleF(1, 1, Width - 3, Height - 3);

        // Shadow + card
        ThemeRenderer.DrawCard(g, cardRect, AppTheme.CardRadius);

        // Hover overlay
        if (_isHovered)
        {
            using var brush = new SolidBrush(AppTheme.HoverHighlight);
            ThemeRenderer.FillRoundedRectangle(g, brush, cardRect, AppTheme.CardRadius);
        }

        // Status accent bar at top
        var accentRect = new RectangleF(cardRect.X + 16, cardRect.Y + 12, 4, cardRect.Height - 24);
        using (var brush = new SolidBrush(StatusAccent))
            ThemeRenderer.FillRoundedRectangle(g, brush, accentRect, 2);

        var contentX = cardRect.X + 32;

        // Icon circle
        var iconSize = 36f;
        var iconRect = new RectangleF(contentX, cardRect.Y + 16, iconSize, iconSize);
        using (var brush = new SolidBrush(Color.FromArgb(25, StatusAccent)))
            g.FillEllipse(brush, iconRect);

        using (var brush = new SolidBrush(StatusAccent))
        {
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(_icon, _iconFont, brush, iconRect, sf);
        }

        // Title
        using (var brush = new SolidBrush(AppTheme.TextPrimary))
            g.DrawString(_title, AppTheme.CardTitleFont, brush, contentX + iconSize + 10, cardRect.Y + 18);

        // Detail
        using (var brush = new SolidBrush(AppTheme.TextSecondary))
            g.DrawString(_detail, AppTheme.CardSecondaryFont, brush, contentX + iconSize + 10, cardRect.Y + 40);

        // Status text at bottom
        var statusText = _status switch
        {
            SummaryStatus.Clear => "All clear",
            SummaryStatus.ActionNeeded => "Action available",
            _ => "Click Scan All",
        };
        using (var brush = new SolidBrush(StatusAccent))
            g.DrawString(statusText, AppTheme.BadgeFont, brush, contentX, cardRect.Bottom - 28);
    }

    protected override void OnClick(EventArgs e)
    {
        CardClicked?.Invoke(this, EventArgs.Empty);
        base.OnClick(e);
    }

    protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }
}
