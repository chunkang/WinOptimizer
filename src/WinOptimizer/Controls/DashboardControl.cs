namespace WinOptimizer.Controls;

using WinOptimizer.Controls.Modern;
using WinOptimizer.Theme;

public class DashboardControl : UserControl
{
    private readonly DashboardSummaryCard _securityCard;
    private readonly DashboardSummaryCard _systemCard;
    private readonly DashboardSummaryCard _networkCard;
    private readonly DashboardSummaryCard _browserCard;
    private readonly Label _lastScanLabel;

    public event EventHandler<int>? NavigateToPage;

    public DashboardControl()
    {
        BackColor = Color.White;
        DoubleBuffered = true;

        var p = AppTheme.ContentPadding;

        // Header panel (docked top)
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.White,
        };

        var titleLabel = new Label
        {
            Text = "Dashboard",
            Font = AppTheme.PageTitleFont,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(p, 12),
            AutoSize = true,
        };

        var subtitleLabel = new Label
        {
            Text = "Overview of your system status. Click a card to navigate to that section.",
            Font = AppTheme.PageSubtitleFont,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(p, 52),
            AutoSize = true,
        };

        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(subtitleLabel);

        // Content panel (fills remaining space, scrollable)
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White,
        };

        var cardGap = 16;

        _securityCard = new DashboardSummaryCard
        {
            Icon = "\U0001F6E1",
            Title = "Security Software",
            Location = new Point(p, 8),
        };
        _securityCard.CardClicked += (_, _) => NavigateToPage?.Invoke(this, 1);

        _systemCard = new DashboardSummaryCard
        {
            Icon = "\u2699",
            Title = "System Optimization",
            Location = new Point(p + _securityCard.Width + cardGap, 8),
        };
        _systemCard.CardClicked += (_, _) => NavigateToPage?.Invoke(this, 2);

        _networkCard = new DashboardSummaryCard
        {
            Icon = "\U0001F310",
            Title = "Network Optimization",
            Location = new Point(p, 8 + _securityCard.Height + cardGap),
        };
        _networkCard.CardClicked += (_, _) => NavigateToPage?.Invoke(this, 3);

        _browserCard = new DashboardSummaryCard
        {
            Icon = "\U0001F5D1",
            Title = "Browser Cache",
            Location = new Point(p + _networkCard.Width + cardGap,
                8 + _securityCard.Height + cardGap),
        };
        _browserCard.CardClicked += (_, _) => NavigateToPage?.Invoke(this, 4);

        _lastScanLabel = new Label
        {
            Text = "",
            Font = AppTheme.CardSecondaryFont,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(p,
                8 + (_securityCard.Height + cardGap) * 2 + 8),
            AutoSize = true,
        };

        contentPanel.Controls.AddRange(new Control[]
        {
            _securityCard, _systemCard, _networkCard, _browserCard,
            _lastScanLabel,
        });

        Controls.Add(contentPanel);
        Controls.Add(headerPanel);
    }

    public void UpdateSecurityCard(int count)
    {
        _securityCard.Detail = count > 0 ? $"{count} program(s) detected" : "No issues found";
        _securityCard.Status = count > 0 ? SummaryStatus.ActionNeeded : SummaryStatus.Clear;
    }

    public void UpdateSystemCard(int unapplied)
    {
        _systemCard.Detail = unapplied > 0 ? $"{unapplied} optimization(s) available" : "All optimized";
        _systemCard.Status = unapplied > 0 ? SummaryStatus.ActionNeeded : SummaryStatus.Clear;
    }

    public void UpdateNetworkCard(int unapplied)
    {
        _networkCard.Detail = unapplied > 0 ? $"{unapplied} optimization(s) available" : "All optimized";
        _networkCard.Status = unapplied > 0 ? SummaryStatus.ActionNeeded : SummaryStatus.Clear;
    }

    public void UpdateBrowserCard(long totalBytes)
    {
        if (totalBytes > 0)
        {
            _browserCard.Detail = $"{Services.BrowserCacheCleanupService.FormatBytes(totalBytes)} cache found";
            _browserCard.Status = SummaryStatus.ActionNeeded;
        }
        else
        {
            _browserCard.Detail = "Cache is clean";
            _browserCard.Status = SummaryStatus.Clear;
        }
    }

    public void SetLastScanTime()
    {
        _lastScanLabel.Text = $"Last scanned: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    }
}
