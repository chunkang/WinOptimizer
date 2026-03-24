namespace WinOptimizer.Theme;

public static class AppTheme
{
    // ── Sidebar ──
    public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#1E1E2E");
    public static readonly Color SidebarText = ColorTranslator.FromHtml("#CCCCCC");
    public static readonly Color SidebarActiveBackground = ColorTranslator.FromHtml("#2D2D44");
    public static readonly Color SidebarHoverBackground = ColorTranslator.FromHtml("#262638");
    public static readonly Color SidebarAccent = ColorTranslator.FromHtml("#0078D4");

    // ── Content ──
    public static readonly Color ContentBackground = ColorTranslator.FromHtml("#F3F3F3");
    public static readonly Color CardBackground = Color.White;
    public static readonly Color CardBorder = ColorTranslator.FromHtml("#E0E0E0");

    // ── Text ──
    public static readonly Color TextPrimary = ColorTranslator.FromHtml("#1A1A1A");
    public static readonly Color TextSecondary = ColorTranslator.FromHtml("#666666");
    public static readonly Color TextOnDark = ColorTranslator.FromHtml("#CCCCCC");
    public static readonly Color TextOnAccent = Color.White;

    // ── Accent ──
    public static readonly Color Accent = ColorTranslator.FromHtml("#0078D4");
    public static readonly Color AccentHover = ColorTranslator.FromHtml("#106EBE");
    public static readonly Color AccentPressed = ColorTranslator.FromHtml("#005A9E");

    // ── Status ──
    public static readonly Color StatusGreen = ColorTranslator.FromHtml("#107C10");
    public static readonly Color StatusAmber = ColorTranslator.FromHtml("#CA5010");
    public static readonly Color StatusRed = ColorTranslator.FromHtml("#D13438");
    public static readonly Color Disabled = ColorTranslator.FromHtml("#A0A0A0");

    // ── Misc ──
    public static readonly Color HoverHighlight = Color.FromArgb(20, 0, 0, 0);
    public static readonly Color CheckboxBorder = ColorTranslator.FromHtml("#999999");
    public static readonly Color ShadowColor = Color.FromArgb(15, 0, 0, 0);

    // ── Fonts ──
    public static readonly Font PageTitleFont = new("Segoe UI Semibold", 20f);
    public static readonly Font PageSubtitleFont = new("Segoe UI", 10f);
    public static readonly Font SidebarTitleFont = new("Segoe UI Semibold", 16f);
    public static readonly Font SidebarVersionFont = new("Segoe UI", 8.5f);
    public static readonly Font NavItemFont = new("Segoe UI", 11f);
    public static readonly Font CardTitleFont = new("Segoe UI Semibold", 11f);
    public static readonly Font CardBodyFont = new("Segoe UI", 10f);
    public static readonly Font CardSecondaryFont = new("Segoe UI", 9f);
    public static readonly Font ButtonFont = new("Segoe UI Semibold", 10f);
    public static readonly Font BadgeFont = new("Segoe UI Semibold", 8f);
    public static readonly Font StatusFont = new("Segoe UI", 9f);

    // ── Dimensions ──
    public const int SidebarWidth = 220;
    public const int NavItemHeight = 42;
    public const int CardRadius = 8;
    public const int ButtonRadius = 6;
    public const int CardPadding = 16;
    public const int ContentPadding = 24;
    public const int CheckboxSize = 20;
    public const int StatusBarHeight = 32;
}
