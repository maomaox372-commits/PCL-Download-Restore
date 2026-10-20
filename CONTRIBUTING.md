# Contributing

**English** | [简体中文](CONTRIBUTING.zh-CN.md)

Read [README](README.md), the unchanged upstream [LICENCE](LICENCE) and [license status](LICENSE-STATUS.md). Contribute only material you may share; do not upload unauthorized code, official PCL binaries, game data or account information.

## Report an issue

Include the public assistant version, Windows version, original PCL version and SHA-256, exact steps, expected/actual behavior and a redacted error. Say whether installation, vanilla download, modpacks or restoration is affected. For modpacks, provide public project/file links and Minecraft/loader versions rather than uploading an entire installation. Report third-party patch issues in this repository, not as requests for upstream support.

Remove accounts, tokens, secrets, personal paths and saves from logs. For sensitive issues use [SECURITY.md](SECURITY.md). Do not delete recovery backups while investigating.

## Changes and verification

Preserve exact-hash compatibility checks, path binding, backups, atomic replacement, failure rollback, per-target locking and idempotent restoration. Do not add telemetry, advertisements, weakened account checks or disabled security protections. Keep automatic updates available by default.

Run build.ps1, test.ps1 and tests/StartupNoticeTests.ps1. If patch logic changes, also audit all six affected methods on isolated real files and run the relevant runtime/download checks. Stub tests alone do not verify the real patch. A new target requires its own fingerprint, structural audit and validation; never broaden the whitelist just to accept it.

Use public version **v1a** for this first release. Numeric **1.0.3** remains the internal debugging iteration. Keep English and Simplified Chinese supporting documents aligned; README additionally has Hong Kong and Taiwan Traditional Chinese editions. Preserve exact commands, file names, hashes and original attribution across languages.

PR descriptions should state the problem, resulting behavior, scope, actual checks and untested areas. Rebuilds require new hash records and a new formal release identifier; never replace a published archive silently.