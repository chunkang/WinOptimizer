// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Johnny Kang <abjohnkang@gmail.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public class ModernCheckItem : Control
{
    private bool _isChecked;
    private bool _isHovered;
    private string _statusText = string.Empty;
    private Color _statusColor = AppTheme.TextSecondary;
    private string _description = string.Empty;

    public event EventHandler? CheckedChanged;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            _isChecked = value;
            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; Invalidate(); }
    }

    public Color StatusColor
    {
        get => _statusColor;
        set { _statusColor = value; Invalidate(); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; Invalidate(); }
    }

    public object? ItemTag { get; set; }

    public ModernCheckItem()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = string.IsNullOrEmpty(_description) ? 40 : 54;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        var rect = new RectangleF(0, 0, Width, Height);

        // Hover background
        if (_isHovered && Enabled)
        {
            using var brush = new SolidBrush(AppTheme.HoverHighlight);
            g.FillRectangle(brush, rect);
        }

        // Separator line at bottom
        using (var pen = new Pen(Color.FromArgb(30, 0, 0, 0)))
            g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

        // Checkbox
        var checkRect = new RectangleF(12, 0, AppTheme.CheckboxSize + 4, Height);
        ThemeRenderer.DrawCheckbox(g, checkRect, _isChecked, _isHovered);

        // Text area
        var textX = 12 + AppTheme.CheckboxSize + 12;
        var textWidth = Width - textX - 12;

        if (!string.IsNullOrEmpty(_statusText))
            textWidth -= 80; // reserve space for status badge

        // Title
        var titleY = string.IsNullOrEmpty(_description) ? (Height - 18) / 2f : 8f;
        using (var brush = new SolidBrush(Enabled ? AppTheme.TextPrimary : AppTheme.Disabled))
            g.DrawString(Text, AppTheme.CardBodyFont, brush, new RectangleF(textX, titleY, textWidth, 20),
                new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

        // Description
        if (!string.IsNullOrEmpty(_description))
        {
            using var brush = new SolidBrush(AppTheme.TextSecondary);
            g.DrawString(_description, AppTheme.CardSecondaryFont, brush,
                new RectangleF(textX, titleY + 20, textWidth + 80, 18),
                new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
        }

        // Status badge
        if (!string.IsNullOrEmpty(_statusText))
        {
            var statusSize = g.MeasureString(_statusText, AppTheme.BadgeFont);
            var badgeX = Width - statusSize.Width - 24;
            var badgeY = (Height - 18) / 2f;
            ThemeRenderer.DrawBadge(g, _statusText, badgeX, badgeY, _statusColor);
        }
    }

    protected override void OnClick(EventArgs e)
    {
        if (Enabled)
            IsChecked = !IsChecked;
        base.OnClick(e);
    }

    protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && Enabled)
        {
            IsChecked = !IsChecked;
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData == Keys.Space || base.IsInputKey(keyData);
}
