namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public class ModernCard : Panel
{
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set { _title = value; UpdateLayout(); Invalidate(); }
    }

    public Panel ContentPanel { get; }

    public ModernCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        Padding = new Padding(0);

        ContentPanel = new Panel
        {
            BackColor = Color.Transparent,
            AutoSize = false,
        };
        Controls.Add(ContentPanel);
    }

    private int TitleAreaHeight => string.IsNullOrEmpty(_title) ? 0 : 40;

    private void UpdateLayout()
    {
        var p = AppTheme.CardPadding;
        var titleH = TitleAreaHeight;
        ContentPanel.Location = new Point(p, p + titleH);
        ContentPanel.Size = new Size(Width - p * 2, Height - p * 2 - titleH);
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        UpdateLayout();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        var cardRect = new RectangleF(1, 1, Width - 3, Height - 3);
        ThemeRenderer.DrawCard(g, cardRect, AppTheme.CardRadius);

        if (!string.IsNullOrEmpty(_title))
        {
            var titleRect = new RectangleF(
                cardRect.X + AppTheme.CardPadding,
                cardRect.Y + 10,
                cardRect.Width - AppTheme.CardPadding * 2,
                28);
            using var brush = new SolidBrush(AppTheme.TextPrimary);
            g.DrawString(_title, AppTheme.CardTitleFont, brush, titleRect);
        }
    }
}
