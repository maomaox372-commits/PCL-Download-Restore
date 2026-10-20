param([switch]$Package)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
if (-not (Test-Path -LiteralPath (Join-Path $framework 'csc.exe'))) { $framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319' }
$compiler = Join-Path $framework 'csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw '需要 Windows .NET Framework 4.8（PCL 本身也使用此运行时）。' }
$output = Join-Path $projectRoot 'artifacts'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$cecil = Join-Path $projectRoot 'lib\Mono.Cecil.dll'
if (-not (Test-Path -LiteralPath $cecil)) { $cecil = Join-Path $projectRoot 'tools\cecil\lib\net40\Mono.Cecil.dll' }
if (-not (Test-Path -LiteralPath $cecil)) { throw '缺少 Mono.Cecil.dll；请保留压缩包中的 lib 目录。' }
if ((Get-FileHash -LiteralPath $cecil -Algorithm SHA256).Hash -ne 'C41BDB9FFD3C5F6E17D2382C1012D73703E035E3F1100245FDD4E08C8DC6EB5B') { throw 'Mono.Cecil.dll hash mismatch.' }
Copy-Item -LiteralPath $cecil -Destination (Join-Path $output 'Mono.Cecil.dll') -Force
$compilerArgs = @(
    '/nologo', '/utf8output', '/codepage:65001', '/optimize+', '/target:winexe', '/platform:anycpu',
    ('/out:' + (Join-Path $output 'PCL.DownloadRestore.exe')),
    ('/reference:' + $cecil), '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll', '/reference:System.Xml.dll', '/reference:System.Core.dll',
    ('/reference:' + (Join-Path $framework 'WPF\PresentationFramework.dll')),
    ('/reference:' + (Join-Path $framework 'WPF\WindowsBase.dll')),
    (Join-Path $projectRoot 'src\PatchCore.cs'), (Join-Path $projectRoot 'src\Installer.cs'), (Join-Path $projectRoot 'src/StartupNotice.cs'), (Join-Path $projectRoot 'src/AssemblyInfo.cs')
)
& $compiler @compilerArgs
if ($LASTEXITCODE -ne 0) { throw '编译失败。' }
Copy-Item -LiteralPath (Join-Path $projectRoot 'src\App.config') -Destination (Join-Path $output 'PCL.DownloadRestore.exe.config') -Force
Write-Output ('已编译：' + (Join-Path $output 'PCL.DownloadRestore.exe'))
if ($Package) { & (Join-Path $projectRoot 'package.ps1') }

# Recompilation can change the executable SHA-256. Refresh all release checksum
# documentation before distributing rebuilt binaries.
