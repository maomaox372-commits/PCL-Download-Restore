# Publish v1a on GitHub

**English** | [简体中文](PUBLISHING.zh-CN.md)

Repository: https://github.com/maomaox372-commits/PCL-Download-Restore . Files are prepared locally; upload/publication is not claimed. Use your signed-in browser without sharing passwords or tokens.

## Two deliverables

- **GitHub source:** upload the contents of publication/github/PCL-Download-Restore. The separate PCL-Download-Restore-GitHub-Source-v1a.zip is a portable source copy; extract it before uploading.
- **Cloud drive:** upload both files in publication/网盘分享-v1a: PCL-Download-Restore-v1a.zip and the external Simplified Chinese README.md.

Do not upload the development directory. source-files.txt defines the clean source set, excluding PCL/game binaries, accounts, downloaded fixtures, credentials and local configuration. Web upload does not apply .gitignore for you.

## Upload source

If the repository already exists, use Add file → Upload files. Otherwise create PCL-Download-Restore at https://github.com/new, choose Public to make source public, and do not generate a replacement README/license/gitignore. Keep the original LICENCE and LICENSE-STATUS.md; do not replace them with MIT/GPL.

Upload all contents from the clean source folder, preserving src, tests, docs, lib, licenses and .github. The root README.md is English. README.zh-CN.md, README.zh-HK.md and README.zh-TW.md provide language switches. Supporting documents are English by default with .zh-CN.md companions; both issue templates and the bilingual PR template are included. UI language is still Simplified Chinese.

Commit message: Release v1a source and documentation. Confirm language links work, files are visible and .github/workflows/build.yml exists. The Windows workflow builds and tests, but never downloads PCL/game files or publishes automatically. Only a successful GitHub run establishes hosted CI success.

## Create the first formal release

Open Releases → Create/Draft a new release:

- Tag: **v1a**, targeting the commit containing this source.
- Title: **PCL Download Restore v1a — First release (independent third-party)**.
- Body: docs/releases/v1a.md, or docs/releases/v1a.zh-CN.md. Copy the final ZIP size/hash from the external README.
- Assets: PCL-Download-Restore-v1a.zip, external README.md and publication/SHA256SUMS-v1a.txt.

Save a draft to review if desired, then Publish release. Keep unsigned status and exact compatibility visible. Automatic Source code ZIPs are not the installer. Numeric 1.0.x files were internal iterations, not earlier formal releases; do not create v1.0.3 or vv1a tags. Public version numbering follows actual releases.

## Cloud-drive sharing and final check

Upload the ZIP plus external README together. Describe v1a as an independent tool by Kuro_Miaow for only the exact supported PCL 2.99.0.0 hash, with backup/restore and unsigned status. Do not claim official endorsement, all-version compatibility or guaranteed safety.

After publication, download assets again and verify hashes against the independently published records. Check English/default README, Chinese navigation, original license/attribution and source availability. No browser upload has been performed by this preparation task.

## Rebuild policy

Internal assembly/file version is 1.0.3.0; public product version and release tag are v1a. If changing files, use a new formal version rather than overwriting released assets. Compile first, refresh EXE hashes in all current language/security/release documents, then package and calculate final ZIP/external README hashes. package.ps1 rejects stale EXE records, product-version mismatch and existing ZIPs. Source share templates retain final-ZIP placeholders to avoid self-reference; the external README must contain resolved values.

GitHub references: https://docs.github.com/en/repositories/working-with-files/managing-files/adding-a-file-to-a-repository and https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository?tool=webui . UI labels may change.