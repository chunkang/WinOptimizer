# WinOptimizer

WinOptimizer is designed to help diagnose and resolve issues that may be slowing down your Microsoft Windows system. Its main purpose is to identify unnecessary or problematic software, optimize system settings, and help restore your PC to smoother, faster performance.

## Download

Download the latest `WinOptimizer.exe` from the [Releases](../../releases/latest) page — no build or install required. Just run it as Administrator.

## Features

### 1. Korean Banking/Security Software Detection & Removal

Detects security utilities installed for banking processes and offers to remove them, as they often consume excessive memory and CPU resources. Supported software includes:

| Vendor | Software |
|--------|----------|
| AhnLab | Safe Transaction |
| INCA Internet | nProtect Online Security, nProtect Netizen, nProtect KeyCrypt |
| Initech | INISAFE CrossWeb EX/EX Vina |
| RaonSecure | TouchEn nxKey, TrustZone |
| DreamSecurity | MagicLine4NX, DreamCert, SecureWeb |
| SoftForum | XecureWeb, XecureExpress, SafeGuard |
| KSign | Korea Information Certificate Authority |
| Wizvera | VeraPort |
| SoftCamp | Secure KeyStroke |
| Ubitech | UbiKey |
| SGA | SGNAC/SGV3, SecureDoc |
| Wiz Information | eWiz |
| KICA | eSign |
| AlphaSecure | AlphaShield |
| MarkAny | MarkAny |
| Coscom | SignKorea |
| Hancom | GPKI Tool |
| Others | KTBNet, AnySign4PC, AnySign4PC EX, AnyBank, CrossCert SecureTool, EpageSAFER, SCGuard, SsenStone, WESS SignEx, Nexess NexGuard, McAfee Secure Bank, TrendMicro PC Web Security |

### 2. System Optimization & Disk Cleanup

Optimizes Windows system settings via registry tweaks and cleans up temporary files:

**Registry optimizations:**

- Disabling unnecessary startup processes
- Disabling telemetry and data collection
- Enabling system performance features
- Disabling unnecessary animation effects
- Customizing power management and memory settings

**Example registry optimizations:**

| Setting | Registry Path | Value |
|---------|--------------|-------|
| Disable Windows tips | `HKCU\...\ContentDeliveryManager` → `ShowTips` | `0` |
| Disable Cortana | `HKLM\...\Policies\Microsoft\Windows\Windows Search` → `AllowCortana` | `0` |
| Turn off transparency | `HKCU\...\Themes\Personalize` → `EnableTransparency` | `0` |
| Speed up menus | `HKCU\Control Panel\Desktop` → `MenuShowDelay` | `0` |
| Disable startup programs | `HKCU\...\CurrentVersion\Run` and `HKLM\...\CurrentVersion\Run` | Remove entries |

**Disk cleanup tasks:**

| Task | Location | Description |
|------|----------|-------------|
| Empty Recycle Bin | System-wide | Permanently removes all items in the Recycle Bin |
| Clean Windows Temp | `C:\Windows\Temp` | Deletes system-level temporary files |
| Clean User Temp | `%LOCALAPPDATA%\Temp` | Deletes user-level temporary files |

> **Note:** Locked or in-use files are safely skipped during cleanup. Always back up your registry before making changes. WinOptimizer prompts you to create a restore point before applying optimizations.

### 3. Browser Cache Cleanup

Cleans up browser cache for improved performance and freed disk space, without affecting bookmarks or saved passwords.

**Supported browsers:** Edge, Chrome, Firefox, Opera, Brave

**What gets cleaned:**
- Browser cache (temporary internet files)
- Cookies (optional keep/delete)
- Download history
- Browsing history (optional)
- Other temporary browser data

**Typical cache locations:**

| Browser | Path |
|---------|------|
| Edge | `%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Cache` |
| Chrome | `%LOCALAPPDATA%\Google\Chrome\User Data\Default\Cache` |
| Firefox | `%LOCALAPPDATA%\Mozilla\Firefox\Profiles\<profile>\cache2` |
| Opera | `%APPDATA%\Opera Software\Opera Stable\Cache` |
| Brave | `%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Cache` |

> **Tip:** Close all browsers before running the cache cleanup to avoid file access errors.

### 4. Network (Ethernet) Performance Optimization

Optimizes Ethernet settings to improve speed and stability:

- **Disable Large Send Offload (LSO)** — reduces CPU overhead on some hardware
- **Enable/tune Receive Side Scaling (RSS)** — efficient packet processing on multicore CPUs
- **Disable Energy Efficient Ethernet (EEE)** — prevents latency from power-saving features
- **Set Speed & Duplex manually** — fixed speed (e.g., 1 Gbps Full Duplex) may improve reliability
- **Optimize TCP parameters via registry**

**Registry tweaks:**

| Path | Value | Setting |
|------|-------|---------|
| `...\Services\Tcpip\Parameters` | `TcpAckFrequency` → `1` | Reduce ACK latency |
| `...\Services\Tcpip\Parameters` | `TcpNoDelay` → `1` | Disable Nagle's algorithm |
| `...\Services\LanmanWorkstation\Parameters` | `DisableBandwidthThrottling` → `1` | Remove throttling |
| `...\Services\LanmanWorkstation\Parameters` | `DisableLargeMtu` → `0` | Allow large MTU |

> **Warning:** Advanced network/registry changes may impact system or network stability. Always document your changes and back up settings before proceeding.

## System Requirements

- **.NET 8.0 SDK** or later
- **Windows 10/11** (Windows Forms requires a Windows environment)
- **Visual Studio 2022** (recommended) or the `dotnet` CLI
- **Administrator privileges** required at runtime (UAC elevation via `app.manifest`)

## Build

```bash
# Build
dotnet build src/WinOptimizer/WinOptimizer.csproj -c Release

# Publish single-file self-contained executable
dotnet publish src/WinOptimizer/WinOptimizer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
