// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-10-07)
// ============================================================================

namespace WinOptimizer.Services;

using Microsoft.Win32;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class NetworkOptimizerService
{
    private const string TcpipParametersPath = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
    private const string TcpipInterfacesPath = TcpipParametersPath + @"\Interfaces";

    public List<NetworkSetting> GetSettings()
    {
        var settings = new List<NetworkSetting>
        {
            // TcpAckFrequency and TcpNoDelay are only honored per interface, not globally
            new()
            {
                Name = "TCP ACK Frequency",
                Description = "Set TcpAckFrequency to 1 on each connected adapter to reduce latency",
                RegistryPath = TcpipInterfacesPath,
                ValueName = "TcpAckFrequency",
                OptimizedValue = 1,
                DefaultValue = null, // Key may not exist by default
                ValueKind = RegistryValueKind.DWord,
                PerInterface = true,
            },
            new()
            {
                Name = "TCP No Delay",
                Description = "Disable Nagle's algorithm on each connected adapter for lower latency",
                RegistryPath = TcpipInterfacesPath,
                ValueName = "TcpNoDelay",
                OptimizedValue = 1,
                DefaultValue = null,
                ValueKind = RegistryValueKind.DWord,
                PerInterface = true,
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
            var paths = GetTargetPaths(setting);
            setting.IsApplied = paths.Count > 0 && paths.All(path =>
            {
                var currentValue = RegistryHelper.ReadValue(path, setting.ValueName);
                return currentValue != null &&
                       currentValue.ToString() == setting.OptimizedValue.ToString();
            });
        }

        return settings;
    }

    public (int applied, List<string> errors) ApplySettings(IEnumerable<NetworkSetting> settings)
    {
        var applied = 0;
        var errors = new List<string>();

        foreach (var setting in settings)
        {
            var paths = GetTargetPaths(setting);
            if (paths.Count == 0)
            {
                errors.Add($"Failed to apply: {setting.Name} (no connected network adapter found)");
                continue;
            }

            var allWritten = true;
            foreach (var path in paths)
            {
                allWritten &= RegistryHelper.WriteValue(path, setting.ValueName,
                    setting.OptimizedValue, setting.ValueKind);
            }

            if (allWritten)
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
            var allReverted = true;
            foreach (var path in GetTargetPaths(setting))
            {
                allReverted &= setting.DefaultValue == null
                    ? RegistryHelper.DeleteValue(path, setting.ValueName)
                    : RegistryHelper.WriteValue(path, setting.ValueName, setting.DefaultValue, setting.ValueKind);
            }

            // Earlier versions wrote per-interface values to the global Tcpip\Parameters key,
            // where they have no effect; clean those up as well
            if (setting.PerInterface)
                RegistryHelper.DeleteValue(TcpipParametersPath, setting.ValueName);

            if (allReverted)
            {
                setting.IsApplied = false;
                reverted++;
            }
            else
            {
                errors.Add($"Failed to revert: {setting.Name}");
            }
        }

        return (reverted, errors);
    }

    private static List<string> GetTargetPaths(NetworkSetting setting)
    {
        if (!setting.PerInterface)
            return new List<string> { setting.RegistryPath };

        return RegistryHelper.GetSubKeyNames(setting.RegistryPath)
            .Select(guid => $@"{setting.RegistryPath}\{guid}")
            .Where(HasIPv4Address)
            .ToList();
    }

    // Only adapters that have an address; skips disconnected and unused virtual adapters
    private static bool HasIPv4Address(string interfacePath)
    {
        var dhcpAddress = RegistryHelper.ReadValue(interfacePath, "DhcpIPAddress") as string;
        var staticAddresses = RegistryHelper.ReadValue(interfacePath, "IPAddress") as string[];
        return IsAssigned(dhcpAddress) || (staticAddresses?.Any(IsAssigned) ?? false);
    }

    private static bool IsAssigned(string? address) =>
        !string.IsNullOrEmpty(address) && address != "0.0.0.0";
}
