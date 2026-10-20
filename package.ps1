param([ValidatePattern('^([0-9]+[.][0-9]+[.][0-9]+([.-][A-Za-z0-9.-]+)?|v[0-9]+[a-z][0-9]*)$')][string]$Version='v1a',[string]$OutputDirectory=(Join-Path $PSScriptRoot 'dist'))
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
# Refuse stale checksum documentation before creating any release archive.
$executable = Join-Path $root 'artifacts/PCL.DownloadRestore.exe'
$metadata = [Diagnostics.FileVersionInfo]::GetVersionInfo($executable)
if ($metadata.ProductVersion -ne $Version) { throw 'Public product version does not match package version. Build the matching release first.' }
$executableHash = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash
foreach ($document in @('README.md','README.zh-CN.md','README.zh-HK.md','README.zh-TW.md','docs/USER-GUIDE.md','docs/USER-GUIDE.zh-CN.md','docs/SHARE-README.md','docs/SHARE-README.en.md','docs/SECURITY-WARNINGS.md','docs/SECURITY-WARNINGS.zh-CN.md',('docs/releases/' + $Version + '.md'),('docs/releases/' + $Version + '.zh-CN.md'))) {
    $content = [IO.File]::ReadAllText((Join-Path $root $document))
    $hashes = [regex]::Matches($content, '(?s)PCL[.]DownloadRestore[.]exe SHA-256.*?([A-Fa-f0-9]{64})')
    if ($hashes.Count -ne 1 -or $hashes[0].Groups[1].Value -ne $executableHash) { throw ('Stale or missing executable SHA-256: ' + $document) }
}
$name = 'PCL-Download-Restore-' + $Version
[void][IO.Directory]::CreateDirectory($OutputDirectory)
$zipPath = Join-Path $OutputDirectory ($name + '.zip')
if (Test-Path -LiteralPath $zipPath) { throw 'The destination ZIP already exists. Use a new version or a different -OutputDirectory; released files are never overwritten.' }
$staging = Join-Path $root ('artifacts/package-' + [Guid]::NewGuid().ToString('N'))
$release = Join-Path $staging $name
[void][IO.Directory]::CreateDirectory($release)
foreach ($file in @('PCL.DownloadRestore.exe','PCL.DownloadRestore.exe.config','Mono.Cecil.dll')) {
    Copy-Item -LiteralPath (Join-Path $root ('artifacts/' + $file)) -Destination (Join-Path $release $file)
}
$sourceRoot = Join-Path $release 'source'
$allowed = [IO.File]::ReadAllLines((Join-Path $root 'source-files.txt')) | Where-Object { $_ -and -not $_.StartsWith('#') }
foreach ($relative in $allowed) {
    if ([IO.Path]::IsPathRooted($relative) -or $relative.Contains('..')) { throw ('Invalid source path: ' + $relative) }
    $source = Join-Path $root $relative
    $destination = Join-Path $sourceRoot $relative
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw ('Missing source file: ' + $relative) }
    [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination))
    Copy-Item -LiteralPath $source -Destination $destination
}
$guides = @{ 'README.md' = 'docs/USER-GUIDE.zh-CN.md'; 'README.en.md' = 'docs/USER-GUIDE.md' }
foreach ($destination in $guides.Keys) {
    $guide = [IO.File]::ReadAllText((Join-Path $root $guides[$destination]))
    foreach ($file in @('USER-GUIDE.md','USER-GUIDE.zh-CN.md','SECURITY-WARNINGS.md','SECURITY-WARNINGS.zh-CN.md','UPDATE-OPTIONS.md','UPDATE-OPTIONS.zh-CN.md')) {
        $guide = $guide.Replace(('(' + $file + ')'), ('(source/docs/' + $file + ')'))
    }
    foreach ($file in @('TEST-REPORT.md','TEST-REPORT.zh-CN.md','LICENSE-STATUS.md','LICENSE-STATUS.zh-CN.md')) {
        $guide = $guide.Replace(('(../' + $file + ')'), ('(' + $file + ')'))
    }
    [IO.File]::WriteAllText((Join-Path $release $destination),$guide,[Text.UTF8Encoding]::new($false))
}
foreach ($file in @('TEST-REPORT.md','TEST-REPORT.zh-CN.md','LICENSE-STATUS.md','LICENSE-STATUS.zh-CN.md','NOTICE.md','NOTICE.zh-CN.md')) {
    $content = [IO.File]::ReadAllText((Join-Path $root $file))
    $content = $content.Replace('(tests/', '(source/tests/')
    [IO.File]::WriteAllText((Join-Path $release $file),$content,[Text.UTF8Encoding]::new($false))
}
$licenses = Join-Path $release 'licenses'
[void][IO.Directory]::CreateDirectory($licenses)
Copy-Item -LiteralPath (Join-Path $root 'LICENCE') -Destination (Join-Path $licenses 'PCL-LICENCE.txt')
Copy-Item -LiteralPath (Join-Path $root 'licenses/Mono.Cecil-LICENSE.txt') -Destination (Join-Path $licenses 'Mono.Cecil-LICENSE.txt')
$installCmd = '@echo off' + [Environment]::NewLine + 'start "" /wait "%~dp0PCL.DownloadRestore.exe" --quick-install %*' + [Environment]::NewLine
$manageCmd = '@echo off' + [Environment]::NewLine + 'start "" "%~dp0PCL.DownloadRestore.exe"' + [Environment]::NewLine
[IO.File]::WriteAllText((Join-Path $release '一键安装启用.cmd'),$installCmd,[Text.Encoding]::ASCII)
[IO.File]::WriteAllText((Join-Path $release '管理与恢复.cmd'),$manageCmd,[Text.Encoding]::ASCII)
# Check local documentation links before sealing the archive.
foreach ($document in @(Get-ChildItem -LiteralPath $release -Recurse -Force -File -Filter '*.md')) {
    $content = [IO.File]::ReadAllText($document.FullName)
    foreach ($match in [regex]::Matches($content,'!?\[[^\]\r\n]*\]\(([^)\s]+)\)')) {
        $target = $match.Groups[1].Value
        if ($target -match '^(https?://|#|mailto:)') { continue }
        $target = [Uri]::UnescapeDataString(($target -split '#')[0])
        if ($target -and -not (Test-Path -LiteralPath (Join-Path $document.DirectoryName $target))) {
            throw ('Broken package documentation link: ' + $document.FullName + ' -> ' + $target)
        }
    }
}
$sumPath = Join-Path $release 'SHA256SUMS.txt'
$sums = Get-ChildItem -LiteralPath $release -Recurse -Force -File | Sort-Object FullName | ForEach-Object { (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash + '  ' + $_.FullName.Substring($release.Length + 1).Replace('\','/') }
[IO.File]::WriteAllLines($sumPath,[string[]]$sums,[Text.UTF8Encoding]::new($false))
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($staging,$zipPath,[IO.Compression.CompressionLevel]::Optimal,$false)
$zipHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
[IO.File]::WriteAllText(($zipPath + '.sha256'),$zipHash + '  ' + [IO.Path]::GetFileName($zipPath) + [Environment]::NewLine,[Text.UTF8Encoding]::new($false))
Get-Item -LiteralPath $zipPath | Select-Object FullName,Length
Write-Output ('SHA-256: ' + $zipHash)
