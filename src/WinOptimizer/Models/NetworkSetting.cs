// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-10-07)
// ============================================================================

namespace WinOptimizer.Models;

using Microsoft.Win32;

public class NetworkSetting
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RegistryPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public object OptimizedValue { get; set; } = 0;
    public object? DefaultValue { get; set; }
    public RegistryValueKind ValueKind { get; set; } = RegistryValueKind.DWord;

    // When true, RegistryPath is the Tcpip Interfaces key and the value is
    // written to each connected adapter's {GUID} subkey
    public bool PerInterface { get; set; }
    public bool IsApplied { get; set; }
    public bool IsSelected { get; set; }
}
