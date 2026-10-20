param([string]$Installer = (Join-Path $PSScriptRoot '../artifacts/PCL.DownloadRestore.exe'), [string]$LegacyInstaller)
$ErrorActionPreference = 'Stop'
# Test the real compiled engine separately from the interactive Main gate.
$root = Split-Path -Parent $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
if (-not (Test-Path -LiteralPath (Join-Path $framework 'csc.exe'))) { $framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319' }
$probeDir = Join-Path $root 'artifacts/test'
[void][IO.Directory]::CreateDirectory($probeDir)
$probe = Join-Path $probeDir 'ProductionEngineProbe.exe'
& (Join-Path $framework 'csc.exe') /nologo /codepage:65001 /target:exe ('/out:' + $probe) (Join-Path $PSScriptRoot 'ProductionEngineProbe.cs')
if ($LASTEXITCODE -ne 0) { throw 'Production engine probe compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'TestHost.config') -Destination ($probe + '.config') -Force
$case = Join-Path $PSScriptRoot ('production-installer-' + [Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path (Join-Path $case 'PCL'))
$target = Join-Path $case 'PCL.exe'
$ini = Join-Path $case 'PCL/Setup.ini'
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'fixture/PCL.exe') -Destination $target
$utf8 = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText($ini, "UserPreference:keep-this" + [Environment]::NewLine + "SystemSystemUpdate:1" + [Environment]::NewLine, $utf8)
function Invoke-Installer([string]$Command, [int]$Expected = 0, [string]$EngineAssembly = $Installer) {
    $start = New-Object System.Diagnostics.ProcessStartInfo
    $start.FileName = $probe
    $start.Arguments = '"' + [IO.Path]::GetFullPath($EngineAssembly) + '" ' + $Command + ' "' + $target + '"'
    $start.WorkingDirectory = $probeDir
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $start
    try {
        [void]$process.Start()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit(60000)) { $process.Kill(); throw "$Command timed out in isolated test fixture." }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        if ($process.ExitCode -ne $Expected) { throw "$Command returned $($process.ExitCode), expected $Expected. $stdout $stderr" }
    } finally { $process.Dispose() }
}
$original = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
Invoke-Installer '--inspect'
Invoke-Installer '--install'
$patched = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
if ($patched -eq $original) { throw 'Install did not patch executable' }
if (-not [IO.File]::ReadAllText($ini).Contains('SystemSystemUpdate:1')) { throw 'Default install changed official update settings' }
Invoke-Installer '--install'
if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $patched) { throw 'Repeat install mutated executable' }
Invoke-Installer '--install-lock'
if (-not [IO.File]::ReadAllText($ini).Contains('SystemSystemUpdate:3')) { throw 'Explicit update lock failed' }
Invoke-Installer '--install'
if (-not [IO.File]::ReadAllText($ini).Contains('SystemSystemUpdate:1')) { throw 'Reinstall failed to release tracked update lock' }
if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $patched) { throw 'Lock migration changed patched result' }
Copy-Item -LiteralPath $target -Destination (Join-Path $case 'Patched-Audit.exe')
[IO.File]::AppendAllText($ini, 'AfterInstall:preserve' + [Environment]::NewLine, $utf8)
Invoke-Installer '--restore'
if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $original) { throw 'Restore hash mismatch' }
$text = [IO.File]::ReadAllText($ini)
if (-not $text.Contains('SystemSystemUpdate:1') -or -not $text.Contains('AfterInstall:preserve') -or -not $text.Contains('UserPreference:keep-this')) { throw 'Restore failed settings preservation' }
Invoke-Installer '--restore'
Invoke-Installer '--inspect'
[IO.File]::WriteAllBytes($target,[Text.Encoding]::UTF8.GetBytes('simulated-author-update'))
$updated = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
Invoke-Installer '--restore' 1
Invoke-Installer '--install' 1
if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $updated) { throw 'Official update was overwritten' }
if (-not [IO.File]::ReadAllText($ini).Contains('SystemSystemUpdate:1')) { throw 'Official update settings changed' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'fixture/PCL.exe') -Destination $target -Force
[IO.File]::AppendAllText($target, 'tamper')
Invoke-Installer '--install' 1
Write-Output 'PASS: production engine default no-lock/explicit lock/unlock/restore/rerestore; future update preserved; tampered file rejected.'
Write-Output ('Original: ' + $original)
Write-Output ('Patched:  ' + $patched)
Write-Output ('Fixture:  ' + $case)

if ($LegacyInstaller) {
    $legacyCase = Join-Path $PSScriptRoot ('production-installer-' + [Guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory((Join-Path $legacyCase 'PCL'))
    $target = Join-Path $legacyCase 'PCL.exe'
    $ini = Join-Path $legacyCase 'PCL/Setup.ini'
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'fixture/PCL.exe') -Destination $target
    [IO.File]::WriteAllText($ini, "SystemSystemUpdate:1
Other:keep
", $utf8)
    Invoke-Installer '--install-lock' 0 $LegacyInstaller
    if ((Get-FileHash -LiteralPath $target).Hash -ne '2FF3A197EDA8F7E9DF707A0CE910A4338D21E898359E09846938984E877A2A90') { throw 'Legacy patch hash mismatch' }
    Invoke-Installer '--install'
    if ((Get-FileHash -LiteralPath $target).Hash -ne $patched -or -not [IO.File]::ReadAllText($ini).Contains('SystemSystemUpdate:1')) { throw 'Legacy migration failed' }
    Invoke-Installer '--restore'
    Invoke-Installer '--install-lock' 0 $LegacyInstaller
    [IO.File]::WriteAllText($ini, "SystemSystemUpdate:2
Other:edited
", $utf8)
    Invoke-Installer '--install'
    if (-not [IO.File]::ReadAllText($ini).Contains('SystemSystemUpdate:2') -or -not [IO.File]::ReadAllText($ini).Contains('Other:edited')) { throw 'Legacy migration overwrote later user preferences' }
    Invoke-Installer '--restore'
    if ((Get-FileHash -LiteralPath $target).Hash -ne $original) { throw 'Legacy migration final restore mismatch' }
    Write-Output 'PASS: real 1.0.1 engine -> 1.0.2 migration; tracked lock removed; user preferences preserved; original restored.'
}
