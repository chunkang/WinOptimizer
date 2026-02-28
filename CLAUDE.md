# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WinOptimizer is a Windows system diagnostic and optimization tool. It has two main purposes:

1. **Detect and remove Korean banking/security software** (e.g., AhnLab Safe Transaction, nProtect, TouchEn nxKey, INISAFE, MagicLine4NX, etc.) that consumes excessive memory/CPU and degrades system performance.
2. **Optimize Windows system settings** via registry tweaks — disabling telemetry, unnecessary animations, startup programs, and tuning network/Ethernet performance.

## Current State

The repository is in early stages — it contains only a README.md describing the planned features. No source code, build system, or tests exist yet.

## Key Domain Context

- Target platform is Microsoft Windows (registry modifications, Device Manager settings, Windows services)
- The banking/security software list is specific to South Korean financial institution requirements
- Registry modifications should always prompt users to create a restore point before applying changes
- Network optimizations target Ethernet adapter settings and TCP parameters

## Registry Paths Used

- System startup: `HKCU\...\CurrentVersion\Run`, `HKLM\...\CurrentVersion\Run`
- UI/performance: `HKCU\...\ContentDeliveryManager`, `HKCU\...\Themes\Personalize`, `HKCU\Control Panel\Desktop`
- Network/TCP: `HKLM\...\Services\Tcpip\Parameters`, `HKLM\...\Services\LanmanWorkstation\Parameters`
