# Changelog

**English** | [简体中文](CHANGELOG.zh-CN.md)

## v1a — First formal release

Prepared 2026-10-21. The formal release date/status is determined by actual publication. Earlier numeric builds were internal iterations, not public releases. Public product/UI/archive/tag naming is v1a; numeric assembly/file version remains 1.0.3.0 for debugging.

- Restore vanilla and online/local modpack entry points on the exact whitelisted PCL 2.99.0.0 build, retaining original download/install behavior.
- Backups, safe restoration, interrupted-operation recovery, per-target locking and unknown-update protection.
- Updates remain available by default; explicit opt-out has red warnings, Yes/No explanations and manual/source help.
- Blue-and-white manager, original-author attribution and visible third-party notice in patched PCL.
- Every assistant launch shows genuine-Minecraft support, update/re-enable guidance, calculated EXE SHA-256 and repository/mismatch warnings.
- English default README plus Simplified Chinese and Hong Kong/Taiwan Traditional Chinese; supporting documents in English and Simplified Chinese. Separate source/installer archives and external cloud-drive README.
- Unsigned distribution, fresh language-aware hash checks and no changes to system trust.

## Internal development history — not release versions

- **1.0.3:** explicit update selection/help, blue UI and 54 initial UI checks; v1a naming extends these to 60 checks.
- **1.0.2:** default no-lock, safe older-patch migration, preserved official updates/rollback, visible PCL notice and startup hash. 43 transaction and 42 notice checks, six-method audit/JIT and online dispatch.
- **1.0.1:** single-action genuine-game notice, Kuro_Miaow attribution, isolated real-engine tests and 32 notice checks. No signing certificate.
- **1.0.0:** first exact-build download restoration, backup/restore and runtime download fixtures. Later test scripts stopped repeatedly launching the production UI via Shell. Existing internal archives are retained locally for traceability, not distributed as prior formal releases.

Detailed results and limitations are in [TEST-REPORT.md](TEST-REPORT.md).