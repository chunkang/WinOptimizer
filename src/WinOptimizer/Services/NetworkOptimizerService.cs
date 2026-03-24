// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Services;

using Microsoft.Win32;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class NetworkOptimizerService
{
    public List<NetworkSetting> GetSettings()
    {
        var settings = new List<NetworkSetting>
        {
            new()
            {
                Name = "TCP ACK Frequency",
                Description = "Set TcpAckFrequency to 1 to reduce latency",
                RegistryPath = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                ValueName = "TcpAckFrequency",
                OptimizedValue = 1,
                DefaultValue = null, // Key may not exist by default
                ValueKind = RegistryValueKind.DWord,
            },
            new()
            {
                Name = "TCP No Delay",
                Description = "Disable Nagle's algorithm for lower latency",
                RegistryPath = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                ValueName = "TcpNoDelay",
                OptimizedValue = 1,
                DefaultValue = null,
                ValueKind = RegistryValueKind.DWord,
            },
            new()
            {
                Name = "Disable Bandwidth Throttling",
                Description = "Remove network bandwidth limitations",
                RegistryPath = @"HKLM\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters",
                ValueName = "DisableBandwidthThrottling",
                OptimizedValue = 1,
                DefaultValue = null,
                ValueKind = RegistryValueKind.DWord,
            },
            new()
            {
                Name = "Enable Large MTU",
                Description = "Allow large MTU for better throughput",
                RegistryPath = @"HKLM\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters",
                ValueName = "DisableLargeMtu",
                OptimizedValue = 0,
                DefaultValue = null,
                ValueKind = RegistryValueKind.DWord,
            },
        };

        foreach (var setting in settings)
        {
            var currentValue = RegistryHelper.ReadValue(setting.RegistryPath, setting.ValueName);
            setting.IsApplied = currentValue != null &&
                                currentValue.ToString() == setting.OptimizedValue.ToString();
        }

        return settings;
    }

    public (int applied, List<string> errors) ApplySettings(IEnumerable<NetworkSetting> settings)
    {
        var applied = 0;
        var errors = new List<string>();

        foreach (var setting in settings)
        {
            if (RegistryHelper.WriteValue(setting.RegistryPath, setting.ValueName,
                    setting.OptimizedValue, setting.ValueKind))
            {
                setting.IsApplied = true;
                applied++;
            }
            else
            {
                errors.Add($"Failed to apply: {setting.Name}");
            }
        }

        return (applied, errors);
    }

    public (int reverted, List<string> errors) RevertSettings(IEnumerable<NetworkSetting> settings)
    {
        var reverted = 0;
        var errors = new List<string>();

        foreach (var setting in settings)
        {
            if (setting.DefaultValue == null)
            {
                if (RegistryHelper.DeleteValue(setting.RegistryPath, setting.ValueName))
                {
                    setting.IsApplied = false;
                    reverted++;
                }
                else
                {
                    errors.Add($"Failed to revert: {setting.Name}");
                }
            }
            else
            {
                if (RegistryHelper.WriteValue(setting.RegistryPath, setting.ValueName,
                        setting.DefaultValue, setting.ValueKind))
                {
                    setting.IsApplied = false;
                    reverted++;
                }
                else
                {
                    errors.Add($"Failed to revert: {setting.Name}");
                }
            }
        }

        return (reverted, errors);
    }
}
