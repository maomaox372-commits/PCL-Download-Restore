# Update Options, Source Changes and Blue Theme

Original PCL author: 龙腾猫跃 · https://afdian.com/a/LTCat . Independent maintainer: Kuro_Miaow. Public version: **v1a, First release**.

**English** | [简体中文](UPDATE-OPTIONS.zh-CN.md)

## Choose in the manager

Run 管理与恢复.cmd and select a supported PCL. Its single 禁用 PCL 自动更新 checkbox is **unchecked by default**. The adjacent description changes immediately.

- **No (default):** no update lock is imposed; existing settings are preserved. A policy already disabled by the user is not forcibly enabled.
- **Yes (explicit, not recommended):** installation sets SystemSystemUpdate to 3 in PCL/Setup.ini beside the target. **Official automatic updates are disabled; author fixes/rollbacks may be missed.**
- Checking alone changes no files. Click 安装并启用 to apply. The option describes this installation request, not detection of installed state, and is unchecked on each manager start.

**Keep the default so official updates/rollbacks can replace the patch. A future PCL2 update may require a new tool/script or re-enabling this one only if compatible. Unknown builds are rejected.** Quick installation and normal CLI do not disable updates.

## Release a tracked lock

Close PCL. Keep its original path and .pcl-download-restore. Uncheck the option and install again, or choose 恢复原版. The tool restores its tracked setting, preserving later user edits. If the original policy was already disabled, choose the desired update mode in PCL settings yourself. Migration restores first and reinstalls second; failure in stage two leaves the original. Do not delete backups or edit recovery XML. There is no background guard, interception or automatic repatching.

## Edit configuration manually

Prefer the manager to preserve recovery consistency. If editing: close PCL, back up PCL/Setup.ini, locate SystemSystemUpdate in a text editor. Value 3 means disabled. Restore **your own original value**, or remove the line if it originally did not exist. **Do not guess other numeric meanings.** Alternatively choose an update mode inside PCL. Preserve other keys, encoding and line endings; never edit recovery XML. Later manual changes may be retained by the assistant as user choices.

## Edit source and rebuild

In the installer archive enter source/; in GitHub source use the root. Locate these methods in src/Installer.cs:

- InstallerForm: lockUpdates.Checked = false is the GUI default; UpdateChoiceDescription() supplies Yes/No text.
- InstallerProgram.QuickInstall: Install(target, false) is the quick default.
- InstallerProgram.Main: only explicit --lock-updates passes true; --no-update-lock stays compatible.
- IniDocument.SetLock: writes SystemSystemUpdate:3. Keep backup/restore logic intact.

Do not change defaults to true. If customizing, align UI/help/README, test expectations and version metadata. Do not loosen hashes, version checks or upstream update logic/URLs to bypass compatibility protection.

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./build.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./test.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./tests/StartupNoticeTests.ps1

Recompilation changes hashes. Refresh all current README/release/checksum records before packaging a new formal version. Public version is v1a; assembly/file version 1.0.3.0 records internal iteration. A self-reported hash is not authentication.

## Colors and layout

InstallerForm.ThemeBlue is #0066CC; ThemeBackground is #F0F7FD. ActionButton() sets borders/hover/pressed colors. StartupNotice.cs uses the same palette and retains only 确定(Y). **Risk text stays #C00000 red.** The assistant does not change PCL's own theme or imply official UI.

TableLayoutPanel, docking, wrapping labels and scrollable help support resizing. Default/minimum windows have off-screen checks; not every DPI/font/system-theme combination was tested.