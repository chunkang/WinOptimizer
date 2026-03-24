// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-03-24)
// ============================================================================

namespace WinOptimizer.Helpers;

using Microsoft.Win32;

public static class RegistryHelper
{
    public static object? ReadValue(string fullPath, string valueName)
    {
        try
        {
            ParsePath(fullPath, out var hive, out var subKey);
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKey);
            return key?.GetValue(valueName);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error reading registry {fullPath}\\{valueName}: {ex.Message}");
            return null;
        }
    }

    public static bool WriteValue(string fullPath, string valueName, object value, RegistryValueKind kind)
    {
        try
        {
            ParsePath(fullPath, out var hive, out var subKey);
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.CreateSubKey(subKey, true);
            if (key == null) return false;
            key.SetValue(valueName, value, kind);
            LogHelper.Log($"Registry write: {fullPath}\\{valueName} = {value}");
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error writing registry {fullPath}\\{valueName}: {ex.Message}");
            return false;
        }
    }

    public static bool DeleteValue(string fullPath, string valueName)
    {
        try
        {
            ParsePath(fullPath, out var hive, out var subKey);
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKey, true);
            if (key == null) return false;
            key.DeleteValue(valueName, false);
            LogHelper.Log($"Registry delete: {fullPath}\\{valueName}");
            return true;
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error deleting registry {fullPath}\\{valueName}: {ex.Message}");
            return false;
        }
    }

    public static string[] GetSubKeyNames(string fullPath)
    {
        try
        {
            ParsePath(fullPath, out var hive, out var subKey);
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKey);
            return key?.GetSubKeyNames() ?? Array.Empty<string>();
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error reading subkeys of {fullPath}: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    public static Dictionary<string, object?> GetAllValues(string fullPath)
    {
        var result = new Dictionary<string, object?>();
        try
        {
            ParsePath(fullPath, out var hive, out var subKey);
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKey);
            if (key == null) return result;

            foreach (var name in key.GetValueNames())
            {
                result[name] = key.GetValue(name);
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error reading all values of {fullPath}: {ex.Message}");
        }
        return result;
    }

    public static RegistryKey? OpenSubKey(RegistryHive hive, RegistryView view, string subKey)
    {
        try
        {
            var baseKey = RegistryKey.OpenBaseKey(hive, view);
            return baseKey.OpenSubKey(subKey);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error opening {hive}\\{subKey} ({view}): {ex.Message}");
            return null;
        }
    }

    private static void ParsePath(string fullPath, out RegistryHive hive, out string subKey)
    {
        var separatorIndex = fullPath.IndexOf('\\');
        if (separatorIndex < 0)
            throw new ArgumentException($"Invalid registry path: {fullPath}");

        var root = fullPath[..separatorIndex].ToUpperInvariant();
        subKey = fullPath[(separatorIndex + 1)..];

        hive = root switch
        {
            "HKEY_LOCAL_MACHINE" or "HKLM" => RegistryHive.LocalMachine,
            "HKEY_CURRENT_USER" or "HKCU" => RegistryHive.CurrentUser,
            "HKEY_CLASSES_ROOT" or "HKCR" => RegistryHive.ClassesRoot,
            "HKEY_USERS" or "HKU" => RegistryHive.Users,
            "HKEY_CURRENT_CONFIG" or "HKCC" => RegistryHive.CurrentConfig,
            _ => throw new ArgumentException($"Unknown registry hive: {root}")
        };
    }
}
