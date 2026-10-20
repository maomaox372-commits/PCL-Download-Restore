param([string]$OriginalPcl)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
if (-not (Test-Path -LiteralPath (Join-Path $framework 'csc.exe'))) { $framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319' }
$compiler = Join-Path $framework 'csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework compiler is required.' }
$out = Join-Path $root 'artifacts/test'
[void][IO.Directory]::CreateDirectory($out)
$exe = Join-Path $out 'InstallerTests.exe'
$compilerArgs = @('/nologo','/utf8output','/codepage:65001','/target:exe','/main:InstallerTests',('/out:' + $exe),'/r:System.Drawing.dll','/r:System.Windows.Forms.dll','/r:System.Xml.dll','/r:System.Core.dll',('/r:' + (Join-Path $framework 'WPF/PresentationFramework.dll')),('/r:' + (Join-Path $framework 'WPF/WindowsBase.dll')),(Join-Path $root 'src/Installer.cs'),(Join-Path $root 'src/StartupNotice.cs'),(Join-Path $root 'tests/InstallerTests.cs'))
& $compiler @compilerArgs
if ($LASTEXITCODE -ne 0) { throw 'Installer test compilation failed.' }
Copy-Item -LiteralPath (Join-Path $root 'src/App.config') -Destination ($exe + '.config') -Force
$start = New-Object Diagnostics.ProcessStartInfo
$start.FileName = $exe
$start.WorkingDirectory = $out
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$process = New-Object Diagnostics.Process
$process.StartInfo = $start
try {
    [void]$process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(60000)) { $process.Kill(); throw 'Installer tests timed out after 60 seconds.' }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    Write-Output $stdout
    if ($process.ExitCode -ne 0) { throw ('Installer tests failed: ' + $stderr) }
} finally { $process.Dispose() }
if ($OriginalPcl) {
    $source = (Resolve-Path -LiteralPath $OriginalPcl).ProviderPath
    $hash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
    if ($hash -ne '6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386') { throw 'Original PCL hash does not match the supported build.' }
    $fixtureDir = Join-Path $root 'tests/fixture'
    [void][IO.Directory]::CreateDirectory($fixtureDir)
    $fixture = Join-Path $fixtureDir 'PCL.exe'
    if (Test-Path -LiteralPath $fixture) {
        if ((Get-FileHash -LiteralPath $fixture -Algorithm SHA256).Hash -ne $hash) { throw 'Existing fixture differs; refusing to overwrite it.' }
    } else { Copy-Item -LiteralPath $source -Destination $fixture }
    $installer = Join-Path $root 'artifacts/PCL.DownloadRestore.exe'
    if (-not (Test-Path -LiteralPath $installer)) { throw 'Run build.ps1 before production integration tests.' }
    & (Join-Path $root 'tests/InstallerIntegration.ps1') -Installer $installer
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $hash) { throw 'Source integrity check failed.' }
} else { Write-Output 'Production integration skipped: pass -OriginalPcl with a supported original file to enable it.' }
