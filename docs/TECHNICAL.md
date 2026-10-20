# Implementation and Audit Scope

Original PCL author: 龙腾猫跃 · https://afdian.com/a/LTCat . Independent third-party work by Kuro_Miaow.

**English** | [简体中文](TECHNICAL.zh-CN.md)

Public release: **v1a — First release**. Internal numeric assembly/file version: **1.0.3.0**. Earlier numeric versions describe development only; public numbering follows formal releases.

## Target and method changes

Original PCL 2.99.0.0 SHA-256: 6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386. Reference restriction commit e4b0489a22cfd4bc57a18fc1e2ac66d5c18e7fc1, parent 5ef81e490ae46b1100d61e33d0183094c7a80ca8. Mono.Cecil 0.11.6 net40 is pinned.

RestrictMcDownload is a compile-time constant; optimization removed normal branches, so changing configuration or skipping an exception is insufficient. PatchCore.cs verifies the exact hash/structure and reconstructs affected IL:

1. PageDownloadInstall.LoaderInit(): reconnects the vanilla list loader.
2. ModModpack.ModpackInstall(): restores file selection and existing import delegate.
3. ModModpack.ModpackInstall(string,string,string): restores seven dispatch routes and cleanup/return logic.
4. PageDownloadResourceDetail.InitializeStackResourceVersions(StackPanel): restores modpack install plus save; ordinary mods still save.
5. PageDownloadModify.LoadInstanceFinished(...): restores the Minecraft component card.
6. FormMain.FormMain_Loaded(...): prepends a visible third-party notice and preserves original subsequent instructions.

The known patched SHA-256 is B3061C3E8D95520655045B535817FC524D319C1763C13036F2F903EC65998D05. Audit: 6,339 total methods, these six changed, 6,333 semantically unchanged, original resources and assembly references retained. This is not evidence for other builds.

## Transactions and updates

Installer.cs validates paths/running processes, binds records to full paths, uses a per-target mutex, original backups, staged/flushed atomic replacement, failure rollback and interrupted-operation recovery. By default it does not alter configuration. Explicit locking changes only SystemSystemUpdate, preserving encoding, newlines and unrelated settings.

Known older patches migrate under the same target mutex by Restore then Install. These stages are not jointly atomic; stage-two failure leaves the original. Later user edits are preserved. Unknown official updates are rejected and never replaced. There is no background guard or repatching. GUI/quick/CLI defaults leave updates available according to the existing user policy.

The PCL notice adds six prefix IL instructions, uses the existing PresentationFramework MessageBox and preserves every original instruction, update/account/sponsorship decision and reference. StartupNotice computes the current assistant EXE SHA-256 rather than embedding its final own hash. External release records provide comparison values; self-report is not authentication.

## UI and entry gate

Main shows one StartupNotice before argument branching, and proceeds only after OK. All modes, including --help, are interactive. The numeric metadata is debugging information, not a signature.

InstallerForm uses TableLayoutPanel, docking, wrapping text and a minimum size. ThemeBlue=#0066CC, ThemeBackground=#F0F7FD, white cards and #C00000 red warnings. One unchecked checkbox changes only explanation text until StartAction passes its value to the existing engine. It is not installed-state detection. Help is scrollable; About puts original-author sponsorship first. See [update/source help](UPDATE-OPTIONS.md).

## Verification layers

- test.ps1 compiles Installer.cs + StartupNotice.cs with the InstallerTests.cs PatchCore stub for transaction fault injection. It does not validate the real patch.
- InstallerIntegration.ps1 uses ProductionEngineProbe to invoke the real engine on isolated copies.
- StartupNoticeTests.ps1 checks production Main structure and actual forms, input methods, public/internal versions and calculated hash.
- Audit.ps1 compares original/patched method semantics, references and resources.
- RuntimeProbe is an isolated .NET/WPF host for real PCL download/install methods. No game login or gameplay is performed. TestHost.config affects only its host, not production/system trust.

A new target must repeat the relevant layers with its own fingerprint. PCL/game inputs are supplied by testers and are not shipped or fetched by CI.