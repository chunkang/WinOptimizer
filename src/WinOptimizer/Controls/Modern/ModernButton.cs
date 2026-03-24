// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Johnny Kang <abjohnkang@gmail.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public class ModernButton : Control
{
    private bool _isHovered;
    private bool _isPressed;
    private bool _isPrimary = true;

    public bool IsPrimary
    {
        get => _isPrimary;
        set { _isPrimary = value; Invalidate(); }
    }

    public ModernButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(120, 36);
        Cursor = Cursors.Hand;
        Font = AppTheme.ButtonFont;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        var rect = new RectangleF(0, 0, Width - 1, Height - 1);

        Color bgColor, textColor;
        if (!Enabled)
        {
            bgColor = AppTheme.Disabled;
            textColor = Color.White;
        }
        else if (_isPrimary)
        {
            bgColor = _isPressed ? AppTheme.AccentPressed : _isHovered ? AppTheme.AccentHover : AppTheme.Accent;
            textColor = AppTheme.TextOnAccent;
        }
        else
        {
            bgColor = _isPressed ? Color.FromArgb(30, 0, 0, 0)
                : _isHovered ? Color.FromArgb(15, 0, 0, 0) : Color.Transparent;
            textColor = Enabled ? AppTheme.Accent : AppTheme.Disabled;
        }

        // Background
        using (var brush = new SolidBrush(bgColor))
            ThemeRenderer.FillRoundedRectangle(g, brush, rect, AppTheme.ButtonRadius);

        // Border for secondary
        if (!_isPrimary)
        {
            using var pen = new Pen(Enabled ? AppTheme.Accent : AppTheme.Disabled, 1.5f);
            ThemeRenderer.DrawRoundedRectangle(g, pen, rect, AppTheme.ButtonRadius);
        }

        // Text
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using (var brush = new SolidBrush(textColor))
            g.DrawString(Text, Font, brush, rect, sf);
    }

    protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _isPressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _isPressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
}
