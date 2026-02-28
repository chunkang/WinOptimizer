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
  Models/       - Data classes (DetectedSoftware, OptimizationSetting, NetworkSetting)
  Data/         - KnownSoftwareDatabase (match patterns for ~40 Korean banking apps)
  Helpers/      - RegistryHelper (registry I/O wrapper), AdminHelper, LogHelper
  Services/     - Business logic (SoftwareDetector, Uninstall, RegistryOptimizer, NetworkOptimizer, RestorePoint)
  Forms/        - MainForm (tabbed dialog shell)
  Controls/     - UserControls for each tab (SoftwareDetection, SystemOptimization, NetworkOptimization)
```

### Key Patterns

- **MainForm** hosts a `TabControl` with 3 tabs, each containing a `UserControl`
- All registry access goes through `RegistryHelper` which handles error logging and hive parsing
- Services read current registry state to determine which optimizations are already applied
- Destructive operations (uninstall, registry writes) always prompt for confirmation and offer restore point creation
- Software detection scans 4 registry uninstall paths (HKLM + HKCU, 64-bit + WOW6432Node) with both RegistryView.Registry64 and Registry32
- Operations log to `%LOCALAPPDATA%\WinOptimizer\logs\winoptimizer.log`

## Key Domain Context

- The banking/security software list is specific to South Korean financial institution requirements
- Match patterns are maintained in `Data/KnownSoftwareDatabase.cs`
- Registry modifications always prompt users to create a System Restore point before applying
- Network optimizations may require a system reboot to take effect
