# WinOptimizer
WinOptimizer is designed to help diagnose and resolve issues that may be slowing down your Windows system. Its main purpose is to identify unnecessary or problematic software, optimize system settings, and help restore your PC to smoother, faster performance.

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


## Ask if user wants to delete it for clean environmenet

## Delete all of them, because all of them are consuming heavy memory and CPU, so user laptop is becoming slower

