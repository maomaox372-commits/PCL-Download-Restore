# Reproduce Isolated Runtime Checks

**English** | [简体中文](runtime-reproduce.zh-CN.md)

Windows x64 + .NET Framework 4.8, system csc.exe/C# 5. Never place RuntimeProbe in a real user game/launcher directory. The distribution contains no PCL/game binaries.

## Inputs and command

Supply your own original supported PCL 2.99.0.0: SHA-256 6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386. Patch a copy in an isolated directory. The v1a patched result is B3061C3E8D95520655045B535817FC524D319C1763C13036F2F903EC65998D05; earlier internal 1.0.0/1.0.1 results were 2FF3A197EDA8F7E9DF707A0CE910A4338D21E898359E09846938984E877A2A90.

Keep RuntimeProbe.cs, runtime-reproduce.ps1, runtime-curseforge-manifest.json and runtime-modrinth-index.json together, and supply Mono.Cecil 0.11.6 net40. Adjust these example absolute paths:

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./runtime-reproduce.ps1 -PatchedExe D:/IsolatedTest/PCL.exe -CecilDll D:/Cecil/net40/Mono.Cecil.dll

Add **-SkipDownloads** for JIT and online button-branch checks only. A full run downloads Minecraft 1.20.1 assets/libraries, about 750 MB, and reuses existing files. The script creates only tests/runtime-work, runtime-lib and marked local fixtures.

## Isolation and limits

The host loads PCL's embedded dependencies, creates PCL.Application and WPF resources, clears host Startup/exception events and StartupUri, and never calls PCL Application_Startup/FormMain construction. It neither reads accounts/keys nor modifies the production assembly. FrmMain is an uninitialized field container with a real MyExtraButton for the empty ShowRefresh path; no PCL window appears. Actual download, hash validation, pack parsing and installation remain PCL implementations.

Fixtures are validation data, not playable modpacks:

- CurseForge: Minecraft 1.20.1, Fabric 0.16.14, Wick project 1728334/file 9092522. Metadata comes through PCL's original public DlModRequest route. No API key is extracted or exported.
- Modrinth: Minecraft 1.20.1, Fabric 0.16.14, Sodium 0.5.13, public version OihdIimA metadata.

No Minecraft process or gameplay is tested; dependency completeness/in-game compatibility is not established.

2026-10-20 historical checks passed five original patched-method JITs, Mojang official list, client JAR/JSON, 64 Windows libraries, 3,575 asset objects plus index, nonempty CF/Modrinth installations and online pack/mod dispatch. The update setting was read as 3 through real PCL Settings.Get. Six-method JIT was validated after the third-party notice addition in the internal 1.0.2 stage.

See runtime-summary.json and [test report](../TEST-REPORT.md). Official SHA1 matched client/assets and all 64 Windows libraries; 24 Linux/macOS-only libraries were skipped by platform rules. No manual login or game launch is implied by the online branch checks.