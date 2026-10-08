// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  Chun Kang <kurapa@kurapa.com>
// Modified: Claude (AI-assisted) (2026-10-07)
// ============================================================================

namespace WinOptimizer.Services;

using System.Text.RegularExpressions;
using Microsoft.Win32;
using WinOptimizer.Data;
using WinOptimizer.Helpers;
using WinOptimizer.Models;

public class SoftwareDetectorService
{
    private static readonly (RegistryHive Hive, string SubKey)[] UninstallPaths =
    {
        (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.CurrentUser, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
    };

    // A pattern must start a word (so "eSign" does not match "Designer", "UbiKey" does not
    // match "YubiKey") and must not run on into a lowercase letter (so "eSign" does not match
    // "eSignature"). Digits and capitals may follow, keeping "MagicLine" -> "MagicLine4NX".
    private static readonly (KnownSoftwareEntry Entry, Regex[] Patterns)[] CompiledEntries =
        KnownSoftwareDatabase.Entries
            .Select(entry => (entry, entry.MatchPatterns
                .Select(p => new Regex($"(?<![A-Za-z0-9])(?i:{Regex.Escape(p)})(?![a-z])", RegexOptions.Compiled))
                .ToArray()))
            .ToArray();

    public List<DetectedSoftware> Scan()
    {
        LogHelper.Log("Starting software scan...");
        var results = new List<DetectedSoftware>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (hive, subKey) in UninstallPaths)
        {
            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                ScanRegistryPath(hive, view, subKey, results, seen);
            }
        }

        LogHelper.Log($"Scan complete. Found {results.Count} matching programs.");
        return results;
    }

    private static void ScanRegistryPath(
        RegistryHive hive,
        RegistryView view,
        string subKey,
        List<DetectedSoftware> results,
        HashSet<string> seen)
    {
        using var parentKey = RegistryHelper.OpenSubKey(hive, view, subKey);
        if (parentKey == null) return;

        foreach (var subKeyName in parentKey.GetSubKeyNames())
        {
            try
            {
                using var appKey = parentKey.OpenSubKey(subKeyName);
                if (appKey == null) continue;

                var displayName = appKey.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(displayName)) continue;

                var publisher = appKey.GetValue("Publisher") as string ?? string.Empty;
                var uninstall = appKey.GetValue("UninstallString") as string ?? string.Empty;

                var matchedEntry = FindMatchingEntry(displayName, publisher);
                if (matchedEntry == null)
                    continue;

                var candidate = new DetectedSoftware
                {
                    DisplayName = displayName,
                    Publisher = publisher,
                    UninstallString = uninstall,
                    QuietUninstallString = appKey.GetValue("QuietUninstallString") as string,
                    RegistryKeyPath = appKey.Name,
                    InstallLocation = appKey.GetValue("InstallLocation") as string,
                    DisplayVersion = appKey.GetValue("DisplayVersion") as string,
                    SupportsSilentUninstall = matchedEntry.SupportsSilentUninstall,
                };

                // Deduplicate by display name; keep the entry with the most info
                if (seen.Contains(displayName))
                {
                    var existingIndex = results.FindIndex(r =>
                        r.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
                    if (existingIndex >= 0 && InfoScore(candidate) > InfoScore(results[existingIndex]))
                    {
                        results[existingIndex] = candidate;
                    }
                    continue;
                }

                seen.Add(displayName);
                results.Add(candidate);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error reading subkey {subKeyName}: {ex.Message}");
            }
        }
    }

    private static int InfoScore(DetectedSoftware sw)
    {
        int score = 0;
        if (!string.IsNullOrEmpty(sw.Publisher)) score++;
        if (!string.IsNullOrEmpty(sw.DisplayVersion)) score++;
        if (!string.IsNullOrEmpty(sw.InstallLocation)) score++;
        if (!string.IsNullOrEmpty(sw.UninstallString)) score++;
        if (!string.IsNullOrEmpty(sw.QuietUninstallString)) score++;
        return score;
    }

    private static KnownSoftwareEntry? FindMatchingEntry(string displayName, string publisher)
    {
        foreach (var (entry, patterns) in CompiledEntries)
        {
            foreach (var pattern in patterns)
            {
                if (pattern.IsMatch(displayName) || pattern.IsMatch(publisher))
                    return entry;
            }
        }
        return null;
    }
}
