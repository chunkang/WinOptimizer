namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public class ModernListItem : Control
{
    private bool _isChecked;
    private bool _isHovered;
    private string _publisher = string.Empty;
    private string _version = string.Empty;
    private string _statusText = string.Empty;
    private Color _statusColor = AppTheme.TextSecondary;

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

    public string Publisher
    {
        get => _publisher;
        set { _publisher = value; Invalidate(); }
    }

    public string Version
    {
        get => _version;
        set { _version = value; Invalidate(); }
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

    public object? ItemTag { get; set; }

    public ModernListItem()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 56;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        // Hover background
        if (_isHovered && Enabled)
        {
            using var brush = new SolidBrush(AppTheme.HoverHighlight);
            g.FillRectangle(brush, 0, 0, Width, Height);
        }

        // Separator line
        using (var pen = new Pen(Color.FromArgb(30, 0, 0, 0)))
            g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

        // Checkbox
        var checkRect = new RectangleF(12, 0, AppTheme.CheckboxSize + 4, Height);
        ThemeRenderer.DrawCheckbox(g, checkRect, _isChecked, _isHovered);

        var textX = 12 + AppTheme.CheckboxSize + 12;

        // Name (title)
        using (var brush = new SolidBrush(AppTheme.TextPrimary))
            g.DrawString(Text, AppTheme.CardBodyFont, brush, textX, 8);

        // Publisher + Version
        var detail = _publisher;
        if (!string.IsNullOrEmpty(_version))
            detail += $"  ·  v{_version}";
        using (var brush = new SolidBrush(AppTheme.TextSecondary))
            g.DrawString(detail, AppTheme.CardSecondaryFont, brush, textX, 30);

        // Status badge
        if (!string.IsNullOrEmpty(_statusText))
        {
            var statusSize = g.MeasureString(_statusText, AppTheme.BadgeFont);
            var badgeX = Width - statusSize.Width - 24;
            ThemeRenderer.DrawBadge(g, _statusText, badgeX, (Height - 18) / 2f, _statusColor);
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
}
