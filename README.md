# PCL Download Restore v1a — Independent Third-Party Tool

**Original PCL author: 龙腾猫跃 (LTCat) · [Support the author](https://afdian.com/a/LTCat) · [Official PCL repository](https://github.com/Meloong-Git/PCL)**

**English** | [简体中文](README.zh-CN.md) | [繁體中文（香港）](README.zh-HK.md) | [繁體中文（台灣）](README.zh-TW.md)

**Public version: v1a — First release.** Maintained by **Kuro_Miaow**. Public version numbers follow actual formal releases; earlier numeric builds were internal development iterations.

This independent third-party derivative restores vanilla Minecraft and modpack download entry points in **one exact build of PCL 2.99.0.0**. It is not an official PCL plugin and has no official endorsement. It uses PCL's existing downloader, component installers, instance isolation and accounts. No official or modified PCL EXE, games, mods or account data are distributed. The desktop UI remains Simplified Chinese; language editions translate documentation only.

## Support genuine Minecraft and PCL

**Please purchase genuine Minecraft from [Minecraft's official website](https://www.minecraft.net/) and consider [supporting PCL's author](https://afdian.com/a/LTCat).** This tool provides no game license. Every assistant launch, including CLI and --help, shows a notice with one **确定(Y)** button. Click it or press Y, Alt+Y or Enter. No skip or opt-out is available. Confirmation means only that you have read the notice.

**Official updates or rollbacks can directly replace this patch. Updates are not disabled by default. After a future PCL2 update, you may need a newer script/tool, or to run the current one again if the exact build is compatible. Unrecognized builds are rejected. The assistant neither blocks official replacement nor automatically reapplies patches.**

The patched PCL displays a visible “Kuro_Miaow 第三方修改” notice as its main window loads.

## Download and use

Download **PCL-Download-Restore-v1a.zip** from **Releases → Assets**. GitHub's automatic Source code ZIP and Code → Download ZIP are source and require compilation. Cloud-drive distribution contains the same installer ZIP and a separate Simplified Chinese README.

1. Close PCL and extract the entire ZIP; do not run inside an archive preview.
2. Run **一键安装启用.cmd**, read the notice and confirm. If detection fails, select your compatible PCL EXE, or drop it onto the CMD.
3. Start PCL yourself after installation. Use vanilla automatic installation, online modpacks or local imports.

Run **管理与恢复.cmd** for options and restoration. **禁用 PCL 自动更新** is unchecked by default. Checking it means “Yes, disable automatic updates”; red text warns that official fixes or rollbacks may be missed. The selection takes effect only after **安装并启用** and does not report the installed state. Only this explicit option or --lock-updates writes SystemSystemUpdate:3.

To remove a lock imposed by this tool, close PCL, retain its original path and .pcl-download-restore, uncheck the option and install again. The restore/install process removes the tracked lock, preserving later user choices. A policy you had already disabled is not forcibly enabled. Unknown official update files are never overwritten. See [update options and source editing](docs/UPDATE-OPTIONS.md).

The manager uses blue headings/buttons, a pale-blue background, white cards and red warnings. Requirements: Windows, .NET Framework 4.8 and target-directory write access; no administrator rights, Python or Node.js are needed to use it. See the [user guide](docs/USER-GUIDE.md).

## Verify integrity before running

Release repository: https://github.com/maomaox372-commits/PCL-Download-Restore . Compare the **same version and same file** against an independently obtained release record.

**v1a PCL.DownloadRestore.exe SHA-256:**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**If hashes differ, stop using the file immediately, delete it and obtain it again from the repository. Do not continue running it.**

    Get-FileHash -LiteralPath './PCL.DownloadRestore.exe' -Algorithm SHA256
    Get-FileHash -LiteralPath './PCL-Download-Restore-v1a.zip' -Algorithm SHA256

The startup window calculates the current EXE hash. **A self-reported hash does not authenticate the program.** Verify independently before execution. EXE and ZIP hashes differ; recompilation may change the hash. The archive includes SHA256SUMS.txt for its files. The final ZIP hash is in the external README/release checksum list.

**This release is unsigned: Authenticode NotSigned.** Kuro_Miaow is attribution, not a Windows-verified publisher. Windows security warnings and SmartScreen may still appear. No trust certificates are installed. Do not disable security software or change global trust policies. See [security warnings](docs/SECURITY-WARNINGS.md).

## Exact compatibility and scope

Only this original PCL EXE SHA-256 is accepted:

    6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386

The same version label with another hash is unsupported. Restored entry points include vanilla versions and existing Forge/NeoForge/Fabric options, online modpack installation/save, local CurseForge/Modrinth/HMCL/MMC/MCBBS and other archives already understood by PCL, plus installed-instance Minecraft cards. Ordinary mod resources retain their save behavior.

Removed files, platform restrictions, network failures, corrupt manifests and unsupported components can still fail; success for every modpack is not guaranteed. Accounts, purchase reminders, sponsorship permissions, the PCL theme, certificates and security software are not modified.

## Backups and restoration

Keep .pcl-download-restore beside the target. Operations verify hashes and paths, reject running targets, and use a per-target mutex, staging, atomic replacement and failure rollback. Later unrelated settings and user-changed update values are preserved. Unknown or altered files are rejected.

Recovery records bind to the original full path: **restore before moving or renaming PCL, then reinstall at the new location.** Default installation creates no absent Setup.ini; restoration of a configuration created solely for the lock retains an empty file rather than deleting a user path.

## Validation and development

The finalized checks and their limits are recorded in the [test report](TEST-REPORT.md). Earlier isolated tests downloaded Minecraft 1.20.1 and nonempty Modrinth/CurseForge fixtures. This rename/documentation pass does not repeat network downloads, login or gameplay.

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./build.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./test.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./tests/StartupNoticeTests.ps1

Use the Windows .NET Framework C# compiler. The pinned Mono.Cecil 0.11.6 net40 DLL is included; no online NuGet restore is needed. Numeric assembly/file version **1.0.3.0** is for internal debugging; the public product version and tag are **v1a**. Recompilation changes hashes. Refresh current documents before packaging; package.ps1 rejects stale EXE hashes and existing archives.

CLI: --quick-install [PCL.exe], --install <PCL.exe> [--lock-updates], --restore <PCL.exe>, --inspect <PCL.exe>. The old --no-update-lock is accepted. All modes require confirmation; unattended use is unsupported. Success returns 0, failures nonzero, canceled quick selection 2.

## Source, licensing and feedback

Keep the upstream [LICENCE](LICENCE) unchanged and continue following it. It is not a project-wide MIT/GPL grant; see [license status](LICENSE-STATUS.md) and [attribution](NOTICE.md). Mono.Cecil has a separate [MIT license](licenses/Mono.Cecil-LICENSE.txt). No extra authorization or endorsement is claimed.

Report third-party tool issues here with redacted steps, versions and hashes. Never upload accounts, tokens, saves or full PCL installations. See [contributing](CONTRIBUTING.md), [security](SECURITY.md), [publishing](docs/PUBLISHING.md), [v1a release notes](docs/releases/v1a.md), [technical details](docs/TECHNICAL.md) and [runtime reproduction](tests/runtime-reproduce.md). Supporting documentation has English and Simplified Chinese editions.