// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-04-03)
// ============================================================================

namespace WinOptimizer.Models;

public class DetectedSoftware
{
    public string DisplayName { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string UninstallString { get; set; } = string.Empty;
    public string? QuietUninstallString { get; set; }
    public string RegistryKeyPath { get; set; } = string.Empty;
    public string? InstallLocation { get; set; }
    public string? DisplayVersion { get; set; }
    public bool IsSelected { get; set; }
    public bool SupportsSilentUninstall { get; set; } = true;
}
