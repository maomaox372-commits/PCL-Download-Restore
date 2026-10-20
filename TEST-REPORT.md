# Validation Report

**English** | [简体中文](TEST-REPORT.zh-CN.md)

## v1a — First public release, prepared 2026-10-21

Public ProductVersion and UI: v1a. Internal AssemblyVersion/FileVersion: 1.0.3.0. Release status/date follow actual publication. All tests use Windows/.NET Framework 4.8 and isolated project copies, not the user's PCL/accounts/saves.

- **43 transaction tests passed:** idempotency, tracked update-lock removal, user setting preservation, official update/rollback protection, encoding roundtrips, path/hash validation, rollback, interrupted transactions, mutex and running-target rejection.
- **60 production UI/startup checks passed:** default-off checkbox, Yes/No text, red warning, help navigation, source/config instructions, original-author-first credit, minimum/default layout, separate public/internal versions, one acknowledgment action, click/Y/Alt+Y/Enter, exact live EXE hash and repository/mismatch notice. Production Main was structurally inspected, not exercised as a complete CLI end-to-end flow.
- Real production engine regression passed inspect/default install/reinstall, explicit lock, unlock by reinstall, restore/repeated restore, unknown official-update preservation and tamper rejection. Recovery returns the exact original bytes and preserves later unrelated settings.
- PatchCore and the transaction engine are unchanged from internal 1.0.3; UI changes only identify the formal release. Patched PCL SHA-256 remains B3061C3E8D95520655045B535817FC524D319C1763C13036F2F903EC65998D05. The six-method audit found 6,339 methods, 6 changed and 6,333 semantically unchanged, with original resources/references preserved.
- Final assistant EXE SHA-256: FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3. Company Kuro_Miaow; unsigned/NotSigned. No certificate or system trust changes.
- Forms are rendered off-screen; not every DPI/font combination or manual production click path is covered. Official-update protection uses a simulated replacement, not a future upstream update. No upload, new network download, login or gameplay is claimed in this rename/documentation pass.

## Historical internal checks — not formal release versions

Internal 1.0.3 passed 43 transaction and 54 UI checks, real-engine operations and the six-method audit. Internal 1.0.2 passed 43 transaction and 42 notice checks, real 1.0.1 migration, six-method JIT and online modpack/mod branch checks. Migration preserved backups and user settings; a second-stage failure leaves the original launcher. Six-method JIT included the main-window notice, without opening the user's production PCL.

Internal 1.0.1 had 32 transaction/32 notice checks and a separate real-engine reflection host. Earlier internal 1.0.0 tests exercised production CLI before the interactive notice existed; these are not v1a production CLI tests. Numeric records/hashes in the Simplified Chinese historical report and development draft folder refer only to those internal artifacts.

## Historical runtime downloads, 2026-10-20

Original PCL SHA-256: 6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386. Earlier five-method patch result: 2FF3A197EDA8F7E9DF707A0CE910A4338D21E898359E09846938984E877A2A90, superseded by the six-method result above.

The real PCL vanilla list loader completed using Mojang's official source. Minecraft 1.20.1 client JAR was 23,028,853 bytes, SHA-256 56B71336D2B4FDFFD197F56595B0DA93E32A946F78F382A299B8F4B92758BB0F. Version JSON was 49,384 bytes, SHA-256 7F9A2FFE361D87ED3F1BF98F194A6B9BBB242425418A5FA0BBED8D47957070ED. Download completed with 64 Windows libraries and 3,575 asset objects plus index; official SHA1 values matched.

- **Modrinth nonempty fixture:** MC 1.20.1 + Fabric 0.16.14 + Sodium 0.5.13; installation finished. Sodium: 971,552 bytes, SHA-256 688C26029CE69F0B1F7CF936866656CA6723228AC46C4AEF5B5F793B1E6ABD22. Fabric/intermediary installed and version main class was net.fabricmc.loader.impl.launch.knot.KnotClient.
- **CurseForge nonempty fixture:** MC 1.20.1 + Fabric 0.16.14 + Wick project 1728334/file 9092522. PCL's original public metadata route and loader finished; mod and overrides existed. Wick: 100,533 bytes, SHA-256 F6F1E3E4277C7E53A955876E1E39FCED534F338C47AD197039C949340197A9AF. No API keys were extracted/exported.
- Online list execution bound modpacks to Install_Click plus Save_Click, and ordinary mods to Save_Click. PCL read SystemSystemUpdate=3 in the explicit-lock fixture. These fixtures are validation data, not guaranteed playable packs.

Tests do not cover every game version, loader, pack or external service. Network availability, removed resources, download restrictions and corrupt manifests remain outside local-entry restoration guarantees. No login/gameplay was performed.

## Reproduction

See [runtime instructions](tests/runtime-reproduce.md) and tests/runtime-summary.json. InstallerTests.cs contains a fault-injection PatchCore stub and must not be compiled together with production PatchCore.cs. Real integration/runtime inputs must be supplied independently; no PCL/game/mod binary is shipped. TestHost.config affects only the test host. Rebuilds change hashes, so independently published binary values must be refreshed before packaging.