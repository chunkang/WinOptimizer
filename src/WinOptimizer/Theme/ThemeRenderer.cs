namespace WinOptimizer.Theme;

using System.Drawing.Drawing2D;

public static class ThemeRenderer
{
    public static GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        var diameter = radius * 2;
        var arc = new RectangleF(rect.Location, new SizeF(diameter, diameter));

        // Top-left
        path.AddArc(arc, 180, 90);
        // Top-right
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);
        // Bottom-right
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        // Bottom-left
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

    public static void FillRoundedRectangle(Graphics g, Brush brush, RectangleF rect, float radius)
    {
        using var path = CreateRoundedRectangle(rect, radius);
        g.FillPath(brush, path);
    }

    public static void DrawRoundedRectangle(Graphics g, Pen pen, RectangleF rect, float radius)
    {
        using var path = CreateRoundedRectangle(rect, radius);
        g.DrawPath(pen, path);
    }

    public static void DrawCardShadow(Graphics g, RectangleF cardRect, float radius)
    {
        using var shadowBrush = new SolidBrush(AppTheme.ShadowColor);
        var shadowRect = cardRect;
        shadowRect.Offset(0, 2);
        shadowRect.Inflate(1, 1);
        FillRoundedRectangle(g, shadowBrush, shadowRect, radius + 1);
    }

    public static void DrawCard(Graphics g, RectangleF rect, float radius, bool drawShadow = true)
    {
        if (drawShadow)
            DrawCardShadow(g, rect, radius);

        using var fillBrush = new SolidBrush(AppTheme.CardBackground);
        FillRoundedRectangle(g, fillBrush, rect, radius);

        using var borderPen = new Pen(AppTheme.CardBorder, 1f);
        DrawRoundedRectangle(g, borderPen, rect, radius);
    }

    public static void DrawBadge(Graphics g, string text, float x, float y, Color bgColor)
    {
        var size = g.MeasureString(text, AppTheme.BadgeFont);
        var pillWidth = Math.Max(size.Width + 10, 22);
        var pillHeight = 18f;
        var pillRect = new RectangleF(x, y, pillWidth, pillHeight);

        using var brush = new SolidBrush(bgColor);
        FillRoundedRectangle(g, brush, pillRect, pillHeight / 2);

        using var textBrush = new SolidBrush(AppTheme.TextOnAccent);
        var textRect = new RectangleF(pillRect.X, pillRect.Y, pillRect.Width, pillRect.Height);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(text, AppTheme.BadgeFont, textBrush, textRect, sf);
    }

    public static void DrawCheckbox(Graphics g, RectangleF rect, bool isChecked, bool isHovered = false)
    {
        var size = AppTheme.CheckboxSize;
        var boxRect = new RectangleF(rect.X, rect.Y + (rect.Height - size) / 2, size, size);

        if (isChecked)
        {
            using var fillBrush = new SolidBrush(AppTheme.Accent);
            FillRoundedRectangle(g, fillBrush, boxRect, 4);

            // Draw checkmark
            using var pen = new Pen(Color.White, 2f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            var cx = boxRect.X + size / 2;
            var cy = boxRect.Y + size / 2;
            g.DrawLines(pen, new[]
            {
                new PointF(cx - 4, cy),
                new PointF(cx - 1, cy + 3.5f),
                new PointF(cx + 5, cy - 3.5f),
            });
        }
        else
        {
            var borderColor = isHovered ? AppTheme.Accent : AppTheme.CheckboxBorder;
            using var pen = new Pen(borderColor, 1.5f);
            FillRoundedRectangle(g, Brushes.White, boxRect, 4);
            DrawRoundedRectangle(g, pen, boxRect, 4);
        }
    }

    public static void SetHighQuality(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    }
}
