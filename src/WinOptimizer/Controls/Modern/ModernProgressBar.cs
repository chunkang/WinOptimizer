namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public class ModernProgressBar : Control
{
    private int _value;
    private int _maximum = 100;
    private bool _showPercentage = true;

    public int Value
    {
        get => _value;
        set { _value = Math.Clamp(value, 0, _maximum); Invalidate(); }
    }

    public int Maximum
    {
        get => _maximum;
        set { _maximum = Math.Max(1, value); Invalidate(); }
    }

    public bool ShowPercentage
    {
        get => _showPercentage;
        set { _showPercentage = value; Invalidate(); }
    }

    public ModernProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        Height = 20;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        var barHeight = 6f;
        var barY = (Height - barHeight) / 2f;
        var barRect = new RectangleF(0, barY, Width, barHeight);
        var radius = barHeight / 2f;

        // Track
        using (var trackBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            ThemeRenderer.FillRoundedRectangle(g, trackBrush, barRect, radius);

        // Fill
        if (_value > 0)
        {
            var fraction = (float)_value / _maximum;
            var fillWidth = Math.Max(barHeight, barRect.Width * fraction);
            var fillRect = new RectangleF(barRect.X, barRect.Y, fillWidth, barHeight);

            using var fillBrush = new SolidBrush(AppTheme.Accent);
            ThemeRenderer.FillRoundedRectangle(g, fillBrush, fillRect, radius);
        }

        // Percentage text
        if (_showPercentage && _value > 0)
        {
            var pct = $"{(int)((float)_value / _maximum * 100)}%";
            var textSize = g.MeasureString(pct, AppTheme.BadgeFont);
            using var brush = new SolidBrush(AppTheme.TextSecondary);
            g.DrawString(pct, AppTheme.BadgeFont, brush,
                Width - textSize.Width, barY - textSize.Height - 1);
        }
    }
}
