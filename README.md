# WinOptimizer
WinOptimizer is designed to help diagnose and resolve issues that may be slowing down your Microsoft Windows system. Its main purpose is to identify unnecessary or problematic software, optimize system settings, and help restore your PC to smoother, faster performance.

# Download

Download the latest `WinOptimizer.exe` from the [Releases](../../releases/latest) page — no build or install required. Just run it as Administrator.

# System Requirements

To compile and build WinOptimizer from source, you need:

- **.NET 8.0 SDK** or later
- **Windows 10/11** (Windows Forms requires a Windows environment)
- **Visual Studio 2022** (recommended) or the `dotnet` CLI
- **Administrator privileges** are required at runtime (UAC elevation via `app.manifest`)

### Build

```bash
# Build
dotnet build src/WinOptimizer/WinOptimizer.csproj -c Release

# Publish single-file self-contained executable
dotnet publish src/WinOptimizer/WinOptimizer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

# Features

## Detect security utilities installed for banking processes, such as:
- AhnLab Safe Transaction
- nProtect Online Security (INISAFE, INCA Internet)
- TouchEn nxKey (RaonSecure)
- KSign (Korea Information Certificate Authority)
- XecureWeb (SoftForum)
- MagicLine4NX (DreamSecurity)
- KTBNet
- AnySign4PC
- CrossCert SecureTool
- Wizvera VeraPort
- Veraport (Wizvera)
- INISAFE CrossWeb EX/EX Vina (Initech)
- DreamSecurity SecureWeb
- SGA SGNAC/SGV3
- SoftCamp Secure KeyStroke (SoftCamp)
- UbiKey (Ubitech)
- SoftForum SafeGuard
- eWiz (Wiz Information)
- eSign (KICA)
- AlphaShield (AlphaSecure)
- EpageSAFER
- TrustZone (RaonSecure)
- MarkAny
- AnySign4PC EX
- AnyBank
- Coscom SignKorea
- DreamCert
- SCGuard
- SsenStone
- WESS SignEx
- McAfee Secure Bank
- TrendMicro PC Web Security
- Nexess NexGuard
- SGA SecureDoc
- SoftForum XecureExpress
- INCA nProtect Netizen
- INCA nProtect KeyCrypt
- Hancom GPKI Tool

## Microsoft Windows System Optimizing Features
The WinOptimizer utility can also optimize your Windows system by tweaking certain registry keys. Optimizations may include:
- Disabling unnecessary startup processes
- Disabling telemetry and data collection
- Enabling system performance features
- Disabling unnecessary animation effects
- Customizing power management and memory settings

**Example optimizations (via Windows Registry):**
- Disable Windows tips:
  - `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager`
    - `ShowTips` → `0`
- Disable Cortana:
  - `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search`
    - `AllowCortana` → `0`
- Turn off transparency effects:
  - `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize`
    - `EnableTransparency` → `0`
- Speed up menus:
  - `HKEY_CURRENT_USER\Control Panel\Desktop`
    - `MenuShowDelay` → `0`
- Disable unwanted startup programs by cleaning:
  - `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
  - `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`

**Note:**  
Automated registry modification can improve system performance,
but always backup your registry before changes. WinOptimizer can prompt you to create a restore point before applying these changes.

These system tweaks, in addition to removing unwanted banking/security software, will help maintain a cleaner and faster Windows environment.


## Browser Cache Cleanup
WinOptimizer includes features to clean up browser cache for improved performance and more free disk space. Removing cached data from your web browsers (such as Microsoft Edge, Google Chrome, Mozilla Firefox, and others) helps eliminate unnecessary files, potentially resolving browser slowdowns and freeing up storage.

### Supported Browsers:
- Microsoft Edge
- Google Chrome
- Mozilla Firefox
- Opera
- Brave

### What Gets Cleaned:
- Browser cache (temporary Internet files)
- Cookies (user may be given option to keep or delete)
- Download history
- Browsing history (optional)
- Other temporary browser data

### Typical Cache Locations

**Edge (Chromium-based):**
- `C:\Users\<User>\AppData\Local\Microsoft\Edge\User Data\Default\Cache`

**Chrome:**
- `C:\Users\<User>\AppData\Local\Google\Chrome\User Data\Default\Cache`

**Firefox:**
- `C:\Users\<User>\AppData\Local\Mozilla\Firefox\Profiles\<profile>\cache2`

**Opera:**
- `C:\Users\<User>\AppData\Roaming\Opera Software\Opera Stable\Cache`

**Brave:**
- `C:\Users\<User>\AppData\Local\BraveSoftware\Brave-Browser\User Data\Default\Cache`

> Replace `<User>` with your Windows username and `<profile>` with the appropriate Firefox profile folder.

---

**How WinOptimizer helps:**
- Identifies all installed supported browsers
- Detects cache directories automatically
- Provides an option to safely delete browser cache and temporary files (without affecting bookmarks or saved passwords)
- Offers clear prompts before deletion and can skip cleanup if browser is currently running (to avoid file access errors)

**Benefits of cleaning browser cache:**
- Saves disk space
- May improve browser speed
- Reduces potential privacy risks from old browsing data

> **Tip:** For maximum results, close all browsers before running the cache cleanup.

---

## Network (Ethernet) Performance Optimizations
WinOptimizer can also assist in optimizing your network (specifically Ethernet) settings to improve speed and stability. If you experience sluggish Ethernet performance or network interruptions, you may benefit from the following tweaks:

- **Disable Large Send Offload (LSO):** Reduces CPU overhead but can cause performance issues on some hardware.
- **Enable or tune Receive Side Scaling (RSS):** Allows efficient packet processing on multicore CPUs.
- **Disable Energy Efficient Ethernet (EEE):** Prevents latency introduced by power-saving features.
- **Set Speed & Duplex manually:** Sometimes labeled as "Auto Negotiation"—setting a fixed speed (e.g., 1 Gbps Full Duplex) may improve reliability.
- **Optimize TCP parameters via registry:**
  - Adjust the value of `TcpAckFrequency` to reduce latency for certain applications.
  - Enable or tune TCP Window Auto-Tuning.

**Example network settings to adjust:**
1. Open "Device Manager" → Select your Ethernet adapter → Right-click "Properties" → "Advanced" tab
2. Adjust parameters such as:
   - Large Send Offload: `Disabled`
   - Energy Efficient Ethernet: `Disabled`
   - Speed & Duplex: `1.0 Gbps Full Duplex` (if supported on both ends)

**Registry Tweaks:**
- `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters`
  - `TcpAckFrequency` → `1`
  - `TcpNoDelay` → `1`
- `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters`
  - `DisableBandwidthThrottling` → `1`
  - `DisableLargeMtu` → `0`

> **Warning:**  
> Advanced network/registry changes may impact system or network stability. Always document your changes and consider backing up settings before proceeding.

By tuning these Ethernet settings, you may experience lower latency, better throughput, and improved overall network reliability, especially in demanding environments or when using enterprise applications.


## Ask if user wants to delete it for clean environmenet

## Delete all of them, because all of them are consuming heavy memory and CPU, so user laptop is becoming slower

