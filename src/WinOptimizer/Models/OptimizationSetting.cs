// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Models;

using Microsoft.Win32;

public class OptimizationSetting
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RegistryPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public object OptimizedValue { get; set; } = 0;
    public object? DefaultValue { get; set; }
    public RegistryValueKind ValueKind { get; set; } = RegistryValueKind.DWord;
    public bool IsApplied { get; set; }
    public bool IsSelected { get; set; }
}
