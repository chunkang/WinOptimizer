// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Johnny Kang <abjohnkang@gmail.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Controls.Modern;

using WinOptimizer.Theme;

public class ModernSidebar : Control
{
    public record NavItem(string Label, string Icon);

    private readonly List<NavItem> _items = new();
    private readonly Dictionary<int, string> _badges = new();
    private readonly Font _iconFont = new("Segoe UI", 13f);
    private int _selectedIndex;
    private int _hoveredIndex = -1;
    private string _appVersion = string.Empty;

    public event EventHandler<int>? SelectedIndexChanged;

    // Buttons placed at bottom of sidebar
    public ModernButton BtnScanAll { get; }
    public ModernButton BtnFixAll { get; }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            Invalidate();
            SelectedIndexChanged?.Invoke(this, value);
        }
    }

    public string AppVersion
    {
        get => _appVersion;
        set { _appVersion = value; Invalidate(); }
    }

    public ModernSidebar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Width = AppTheme.SidebarWidth;
        Dock = DockStyle.Left;
        BackColor = AppTheme.SidebarBackground;

        BtnScanAll = new ModernButton
        {
            Text = "Scan All",
            Size = new Size(AppTheme.SidebarWidth - 32, 36),
            IsPrimary = true,
        };

        BtnFixAll = new ModernButton
        {
            Text = "Fix All",
            Size = new Size(AppTheme.SidebarWidth - 32, 36),
            IsPrimary = false,
            Enabled = false,
        };

        // Override secondary button colors for dark bg
        BtnFixAll.ForeColor = AppTheme.SidebarText;

        Controls.Add(BtnScanAll);
        Controls.Add(BtnFixAll);
    }

    public void AddItem(NavItem item) { _items.Add(item); Invalidate(); }

    public void SetBadge(int index, string? text)
    {
        if (text == null) _badges.Remove(index);
        else _badges[index] = text;
        Invalidate();
    }

    private int HeaderHeight => 70;
    private int NavStartY => HeaderHeight + 8;

    private RectangleF GetItemRect(int index)
    {
        return new RectangleF(0, NavStartY + index * AppTheme.NavItemHeight,
            Width, AppTheme.NavItemHeight);
    }

    private int HitTest(Point pt)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            var rect = GetItemRect(i);
            if (rect.Contains(pt)) return i;
        }
        return -1;
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        if (BtnFixAll == null || BtnScanAll == null) return;
        BtnFixAll.Location = new Point(16, Height - 16 - BtnFixAll.Height);
        BtnScanAll.Location = new Point(16, BtnFixAll.Top - 8 - BtnScanAll.Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        ThemeRenderer.SetHighQuality(g);

        g.Clear(AppTheme.SidebarBackground);

        // App title
        using (var brush = new SolidBrush(AppTheme.TextOnAccent))
            g.DrawString("WinOptimizer", AppTheme.SidebarTitleFont, brush, 16, 16);

        // Version
        if (!string.IsNullOrEmpty(_appVersion))
        {
            using var brush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
            g.DrawString($"v{_appVersion}", AppTheme.SidebarVersionFont, brush, 18, 44);
        }

        // Nav items
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var rect = GetItemRect(i);
            var isSelected = i == _selectedIndex;
            var isHovered = i == _hoveredIndex;

            // Background
            if (isSelected)
            {
                using var brush = new SolidBrush(AppTheme.SidebarActiveBackground);
                g.FillRectangle(brush, rect);

                // Accent bar
                using var accentBrush = new SolidBrush(AppTheme.SidebarAccent);
                g.FillRectangle(accentBrush, rect.X, rect.Y + 6, 3, rect.Height - 12);
            }
            else if (isHovered)
            {
                using var brush = new SolidBrush(AppTheme.SidebarHoverBackground);
                g.FillRectangle(brush, rect);
            }

            // Icon
            using (var brush = new SolidBrush(isSelected ? AppTheme.TextOnAccent : AppTheme.SidebarText))
                g.DrawString(item.Icon, _iconFont, brush, rect.X + 18, rect.Y + (rect.Height - 22) / 2);

            // Label
            var textColor = isSelected ? AppTheme.TextOnAccent : AppTheme.SidebarText;
            using (var brush = new SolidBrush(textColor))
                g.DrawString(item.Label, AppTheme.NavItemFont, brush, rect.X + 48, rect.Y + (rect.Height - 18) / 2);

            // Badge
            if (_badges.TryGetValue(i, out var badge))
            {
                var badgeSize = g.MeasureString(badge, AppTheme.BadgeFont);
                var badgeX = Width - badgeSize.Width - 26;
                var badgeY = rect.Y + (rect.Height - 18) / 2;
                ThemeRenderer.DrawBadge(g, badge, badgeX, badgeY, AppTheme.StatusAmber);
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var idx = HitTest(e.Location);
        if (idx != _hoveredIndex)
        {
            _hoveredIndex = idx;
            Cursor = idx >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        if (_hoveredIndex != -1)
        {
            _hoveredIndex = -1;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        var idx = HitTest(e.Location);
        if (idx >= 0)
            SelectedIndex = idx;
        base.OnMouseClick(e);
    }
}
