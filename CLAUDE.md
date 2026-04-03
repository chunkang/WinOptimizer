# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WinOptimizer is a C# Windows Forms (.NET 8.0) dialog-style desktop application. It has two main purposes:

1. **Detect and remove Korean banking/security software** (e.g., AhnLab Safe Transaction, nProtect, TouchEn nxKey, INISAFE, MagicLine4NX, etc.) that consumes excessive memory/CPU and degrades system performance.
2. **Optimize Windows system settings** via registry tweaks — disabling telemetry, unnecessary animations, startup programs, and tuning network/Ethernet performance.

## Build Commands

```bash
# Build
dotnet build src/WinOptimizer/WinOptimizer.csproj -c Release

# Publish single-file self-contained executable
dotnet publish src/WinOptimizer/WinOptimizer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Note: Development may happen on macOS, but building/running the WinForms app requires Windows.

## Architecture

- **Target framework**: `net8.0-windows` with Windows Forms
- **Admin elevation**: `app.manifest` requires `requireAdministrator` via UAC
- **NuGet dependency**: `System.Management` (for WMI restore point creation)

### Layer Structure

```
src/WinOptimizer/
  Models/          - Data classes (DetectedSoftware, OptimizationSetting, NetworkSetting, CleanupTask)
  Data/            - KnownSoftwareDatabase (match patterns for ~40 Korean banking apps)
  Helpers/         - RegistryHelper (registry I/O wrapper), AdminHelper, LogHelper
  Services/        - Business logic (SoftwareDetector, Uninstall, RegistryOptimizer, NetworkOptimizer, SystemCleanup, RestorePoint)
  Theme/           - AppTheme (colors, fonts, spacing) and ThemeRenderer (drawing utilities)
  Controls/Modern/ - Custom-drawn UI controls (ModernSidebar, ModernButton, ModernCard, ModernCheckItem, ModernListItem, etc.)
  Controls/        - Page UserControls (Dashboard, SoftwareDetection, SystemOptimization, NetworkOptimization, BrowserCacheCleanup)
  Forms/           - MainForm (sidebar navigation shell)
```

### Key Patterns

- **MainForm** hosts a `ModernSidebar` + content `Panel` with 5 pages (Dashboard + 4 feature pages)
- All registry access goes through `RegistryHelper` which handles error logging and hive parsing
- Services read current registry state to determine which optimizations are already applied
- Destructive operations (uninstall, registry writes) always prompt for confirmation and offer restore point creation
- Software detection scans 4 registry uninstall paths (HKLM + HKCU, 64-bit + WOW6432Node) with both RegistryView.Registry64 and Registry32
- Operations log to `%LOCALAPPDATA%\WinOptimizer\logs\winoptimizer.log`

## Versioning

- Version is controlled by **`_version.bat`** in the repository root
- Format: `Major.Minor.Build` (e.g., `0.2.5`)
- The `.csproj` `SetVersion` target reads `VERSION_MAJOR`, `VERSION_MINOR`, `VERSION_BUILD` from `_version.bat` at build time
- The title bar displays `WinOptimizer v{version}`
- To change the version, edit the `SET VERSION_*` values in `_version.bat`
- Never hardcode `<Version>` in `.csproj`; it is computed by the `SetVersion` target
- **Rule:** Update `_version.bat` when releasing a new version. Bump build for patches, minor for features, major for breaking changes.

## License

This project is licensed under **AGPL-3.0 + Commons Clause**.

## File Headers

Every `.cs` file must include a comment header at the top. When creating or modifying a file, add or update the header using the following format:

```csharp
// ============================================================================
// WinOptimizer — AGPL-3.0 + Commons Clause
// Author:  <name> <email>
// Modified: <name> <email> (<YYYY-MM-DD>)
// ============================================================================
```

- **Author** is set once when the file is first created and never changed.
- **Modified** is updated every time the file is edited, with the modifier's name and date.
- When Claude Code creates a file, use `Author: Claude (AI-assisted)`.
- When Claude Code modifies an existing file that already has an Author line, keep the original Author and update the Modified line to `Modified: Claude (AI-assisted) (<YYYY-MM-DD>)`.
- If an existing file has no header yet, add one — set Author to the git blame first-commit author if known, otherwise `Unknown`.

## Actions

- **Update README.md on code changes**: Whenever code is modified (new features, removed features, renamed components, changed behavior, etc.), update `README.md` to reflect those changes. Keep the README accurate and in sync with the current state of the codebase.

## Key Domain Context

- The banking/security software list is specific to South Korean financial institution requirements
- Match patterns are maintained in `Data/KnownSoftwareDatabase.cs`
- Registry modifications always prompt users to create a System Restore point before applying
- Network optimizations may require a system reboot to take effect
