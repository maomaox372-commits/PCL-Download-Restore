# PCL Download Restore v1a — User Guide

**Original PCL author: 龙腾猫跃 · Support: https://afdian.com/a/LTCat**

**English** | [简体中文](USER-GUIDE.zh-CN.md)

**v1a — First public release**, maintained independently by Kuro_Miaow. This is a third-party local patch for one exact PCL 2.99.0.0 build, not an official plugin. It restores existing vanilla/modpack entry points and compiled-out dispatch logic using PCL's original downloader and installers.

## Genuine Minecraft and updates

Please buy genuine Minecraft at https://www.minecraft.net/ and support PCL's author. No game license is provided. Every assistant launch, including CLI, requires its single 确定(Y) notice; click or press Y, Alt+Y or Enter. No opt-out/skip is available. Acknowledgment means only that you read it.

**Official updates or rollbacks can directly replace the patch. Updates are not disabled by default. Future PCL2 updates may require a newer script/tool or re-enabling this one on a compatible build. Unrecognized builds are rejected; the assistant does not block replacement or automatically repatch.**

## Integrity and unsigned status

Release source: https://github.com/maomaox372-commits/PCL-Download-Restore . Compare the same version/file against independent release records before running.

**v1a PCL.DownloadRestore.exe SHA-256:**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**If hashes differ, stop immediately, delete the file and obtain it again from the repository.** A startup window's calculated hash cannot authenticate itself. EXE and ZIP values differ; recompilation may change values. Use Get-FileHash before execution. Per-file values are in SHA256SUMS.txt; final ZIP values are external.

**Unsigned: NotSigned.** Kuro_Miaow is attribution, not a Windows-verified publisher. Windows/SmartScreen warnings may remain. Do not install trust certificates or disable security software. See [security warnings](SECURITY-WARNINGS.md).

## Installation

1. Close PCL. Fully extract PCL-Download-Restore-v1a.zip; do not run within an archive preview or move only its EXE.
2. Run **一键安装启用.cmd**, confirm the notice, and select your supported PCL if it is not detected beside the assistant or one directory above. Dragging PCL onto the CMD also works.
3. After success, start PCL yourself and use automatic installation, online modpacks or local imports. The tool does not automatically start/terminate PCL.

Windows + .NET Framework 4.8 and write permission are required. No administrator rights or developer tools are needed. The UI is Simplified Chinese even when you read English documentation.

## Update option and recovery

Run **管理与恢复.cmd**. The checkbox **禁用 PCL 自动更新** is unchecked by default. Checked means disable updates and may miss official fixes/rollback; permanent red text warns about it. The selection is an instruction for the next installation, not detection of current status, and applies only after **安装并启用**. Only an explicit checked selection or --lock-updates writes SystemSystemUpdate:3.

To release this tool's tracked lock, keep the same target path and .pcl-download-restore, uncheck the option and install again, or choose **恢复原版**. Existing user-disabled policies and later user edits are preserved. A migration restores first, then installs; failure in the second stage leaves the original. See [manual configuration/source changes](UPDATE-OPTIONS.md).

Keep the backup folder. Operations verify hashes, reject running/unknown targets and use atomic replacement and rollback. Later unrelated settings are retained. Restore before renaming/moving PCL; the recovery record binds to the full path. If Setup.ini did not exist, a default install does not create it; restoring a lock-created config leaves an empty file.

## Scope and limits

Required original PCL SHA-256:

    6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386

Other builds, previews, modifications and future versions are rejected, even with the same displayed version. No launcher is downloaded or substituted.

Restored functions cover vanilla versions and existing Forge/NeoForge/Fabric options; online modpack install/save; local CurseForge, Modrinth .mrpack, HMCL, MMC, MCBBS and archives PCL already recognizes; installed-instance version cards. Ordinary mods retain save behavior. Platform restrictions, deleted resources, bad manifests, network failures and unsupported components remain possible. Success for every pack is not promised.

The patched PCL visibly announces Kuro_Miaow's third-party changes. Accounts, original purchase/sponsorship checks, permissions, certificates and security software are untouched. No game/mod binaries or account data are bundled. See the [test report](../TEST-REPORT.md).

## Developers and CLI

The installer archive includes source/. The GitHub source root is directly buildable with build.ps1 and the system .NET C# compiler; Mono.Cecil 0.11.6 is pinned and included, with no NuGet restore. Internal numeric file/assembly version is 1.0.3.0; public ProductVersion/tag is v1a.

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./build.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./test.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./tests/StartupNoticeTests.ps1

CLI: --quick-install [PCL.exe], --install <PCL.exe> [--lock-updates], --restore <PCL.exe>, --inspect <PCL.exe>. Old --no-update-lock remains supported. Every mode, including --help, requires manual notice confirmation; unattended operation is unsupported. Success 0, error nonzero, canceled quick selection 2.

## Attribution and redistribution

Original source: https://github.com/Meloong-Git/PCL . Restriction commit: e4b0489a22cfd4bc57a18fc1e2ac66d5c18e7fc1. Keep the upstream license/guidelines, author and sponsorship link. PCL-LICENCE.txt and Mono.Cecil-LICENSE.txt are in licenses/. Mono.Cecil's MIT grant applies to that dependency only. See [license status](../LICENSE-STATUS.md). Users should not run source/tests.