// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Johnny Kang <abjohnkang@gmail.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public class ModernStatusBar : Control
{
    private string _statusText = "Ready";
    private int _progressValue;

    public ModernProgressBar ProgressBar { get; }

    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            Invalidate();
        }
    }

    public int ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = Math.Clamp(value, 0, 100);
            ProgressBar.Value = _progressValue;
            ProgressBar.Visible = _progressValue > 0 && _progressValue < 100;
            Invalidate();
        }
    }

    public ModernStatusBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = AppTheme.StatusBarHeight;
        Dock = DockStyle.Bottom;

        ProgressBar = new ModernProgressBar
        {
            Size = new Size(180, 20),
            Visible = false,
            ShowPercentage = true,
        };
        Controls.Add(ProgressBar);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        if (ProgressBar == null) return;
        ProgressBar.Location = new Point(Width - ProgressBar.Width - 12, (Height - ProgressBar.Height) / 2);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        // Background
        g.Clear(AppTheme.CardBackground);

        // Top border
        using (var pen = new Pen(AppTheme.CardBorder))
            g.DrawLine(pen, 0, 0, Width, 0);

        // Status text
        var textRect = new RectangleF(12, 0, Width - ProgressBar.Width - 36, Height);
        using var sf = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap,
        };
        using var brush = new SolidBrush(AppTheme.TextSecondary);
        g.DrawString(_statusText, AppTheme.StatusFont, brush, textRect, sf);
    }
}
