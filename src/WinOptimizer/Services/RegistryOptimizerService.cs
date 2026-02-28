namespace WinOptimizer.Services;

using Microsoft.Win32;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class RegistryOptimizerService
{
    public List<OptimizationSetting> GetSettings()
    {
        var settings = new List<OptimizationSetting>
        {
            new()
            {
                Name = "Disable Windows Tips",
                Description = "Prevents Windows from showing tips and suggestions",
                Category = "Tips",
                RegistryPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                ValueName = "SoftLandingEnabled",
                OptimizedValue = 0,
                DefaultValue = 1,
                ValueKind = RegistryValueKind.DWord,
            },
            new()
            {
                Name = "Disable Suggested Content",
                Description = "Disables suggested content in Settings app",
                Category = "Tips",
                RegistryPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                ValueName = "SubscribedContent-338389Enabled",
                OptimizedValue = 0,
                DefaultValue = 1,
                ValueKind = RegistryValueKind.DWord,
            },
            new()
            {
                Name = "Disable Cortana",
                Description = "Disables Cortana assistant",
                Category = "Cortana",
                RegistryPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                ValueName = "AllowCortana",
                OptimizedValue = 0,
                DefaultValue = 1,
                ValueKind = RegistryValueKind.DWord,
            },
            new()
            {
                Name = "Disable Transparency Effects",
                Description = "Turns off window transparency for better performance",
                Category = "UI",
                RegistryPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                ValueName = "EnableTransparency",
                OptimizedValue = 0,
                DefaultValue = 1,
                ValueKind = RegistryValueKind.DWord,
            },
            new()
            {
                Name = "Speed Up Menus",
                Description = "Removes menu show delay for faster navigation",
                Category = "UI",
                RegistryPath = @"HKCU\Control Panel\Desktop",
                ValueName = "MenuShowDelay",
                OptimizedValue = "0",
                DefaultValue = "400",
                ValueKind = RegistryValueKind.String,
            },
            new()
            {
                Name = "Disable Animations",
                Description = "Turns off minimize/maximize animations",
                Category = "UI",
                RegistryPath = @"HKCU\Control Panel\Desktop\WindowMetrics",
                ValueName = "MinAnimate",
                OptimizedValue = "0",
                DefaultValue = "1",
                ValueKind = RegistryValueKind.String,
            },
            new()
            {
                Name = "Disable Telemetry",
                Description = "Disables Windows diagnostic data collection",
                Category = "Telemetry",
                RegistryPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                ValueName = "AllowTelemetry",
                OptimizedValue = 0,
                DefaultValue = 1,
                ValueKind = RegistryValueKind.DWord,
            },
        };

        // Read current state for each setting
        foreach (var setting in settings)
        {
            var currentValue = RegistryHelper.ReadValue(setting.RegistryPath, setting.ValueName);
            setting.IsApplied = currentValue != null &&
                                currentValue.ToString() == setting.OptimizedValue.ToString();
        }

        return settings;
    }

    public (int applied, List<string> errors) ApplySettings(IEnumerable<OptimizationSetting> settings)
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

    public (int reverted, List<string> errors) RevertSettings(IEnumerable<OptimizationSetting> settings)
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

    public Dictionary<string, object?> GetStartupEntries(string registryPath)
    {
        return RegistryHelper.GetAllValues(registryPath);
    }
}
