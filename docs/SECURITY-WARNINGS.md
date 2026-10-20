# Windows Warnings and Startup Notices

**English** | [简体中文](SECURITY-WARNINGS.zh-CN.md)

## v1a unsigned release

Authenticode status is **NotSigned**. Kuro_Miaow in properties/UI is attribution, not a verified Windows publisher. No self-signed certificate, trust-certificate installation or Azure signing configuration is included. Windows may show Open File/Security Warning, Unknown publisher or SmartScreen according to origin/policy. Neither a warning nor its absence alone establishes malware/safety.

## Application notice versus system warning

The assistant's blue-and-white “正版支持提示” window encourages genuine Minecraft and author support. Its only action is 确定(Y), accepted by click/Y/Alt+Y/Enter. Each new launch shows it once before normal operation, including all CLI modes. No opt-out exists. It does not request certificates, elevation or changed system security settings.

Older internal regression scripts started the production assistant seven times through Shell per run, potentially producing repeated system prompts. Tests now use a separate real-engine host and off-screen notice checks, with UseShellExecute=false, captured output and time limits. TestHost.config affects only the test host; production/system trust and origin markers remain unchanged. Earlier screenshots had no observed Zone.Identifier, which does not establish every historical cause. Users should not run source/tests.

## Integrity and source

Repository: https://github.com/maomaox372-commits/PCL-Download-Restore . Verify the same release and file against an independently retrieved record.

**v1a PCL.DownloadRestore.exe SHA-256:**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**If a value differs, stop using the file immediately, delete it and obtain it again from the repository.** Do not continue clicking through.

    Get-FileHash -LiteralPath './PCL.DownloadRestore.exe' -Algorithm SHA256
    Get-FileHash -LiteralPath './PCL-Download-Restore-v1a.zip' -Algorithm SHA256

The window calculates its current EXE hash and shows the repository/mismatch warning. **That self-report cannot authenticate the program.** EXE and ZIP hashes are different; locally rebuilt files can differ. SHA256SUMS.txt checks content consistency, not publisher identity or safety. Final archive values must be obtained externally to avoid self-reference.

## If prompts repeat

Cancel, check the shown path and ensure you have not repeatedly launched the assistant or its tests. Verify origin and hashes. Extract all files into a fresh directory. If you have independently chosen to trust a verified download, Windows ZIP Properties may expose an Unblock option; it is optional and may be absent. Do not disable Defender, SmartScreen or UAC, change global registry/trust policy, or unblock whole drives.

The assistant does not automatically trust or delete itself. Official updates remain available by default; opting out may miss security fixes. See [update options](UPDATE-OPTIONS.md).

References: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/unblock-file and https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.useshellexecute?view=netframework-4.8 .