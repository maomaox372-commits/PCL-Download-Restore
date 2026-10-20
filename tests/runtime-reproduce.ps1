param([Parameter(Mandatory=$true)][string]$PatchedExe,[Parameter(Mandatory=$true)][string]$CecilDll,[switch]$SkipDownloads)
$ErrorActionPreference='Stop'
# This script creates disposable fixtures under its own directory only.
$tests=$PSScriptRoot
$work=Join-Path $tests 'runtime-work'
New-Item -ItemType Directory -Force -Path $work | Out-Null
Copy-Item -LiteralPath $CecilDll -Destination (Join-Path $work 'Mono.Cecil.dll') -Force
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$framework=Split-Path -Parent $compiler
$argsToCompiler=@('/nologo','/codepage:65001','/langversion:5','/target:exe',('/out:'+(Join-Path $work 'RuntimeProbe.exe')),('/r:'+(Join-Path $work 'Mono.Cecil.dll')),('/r:'+(Join-Path $framework 'WPF/WindowsBase.dll')),('/r:'+(Join-Path $framework 'WPF/PresentationFramework.dll')),('/r:'+(Join-Path $framework 'WPF/PresentationCore.dll')),('/r:'+(Join-Path $framework 'System.Xaml.dll')),(Join-Path $tests 'RuntimeProbe.cs'))
& $compiler @argsToCompiler
if($LASTEXITCODE -ne 0){throw 'RuntimeProbe compilation failed'}
$config='<configuration><startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" /></startup><runtime><loadFromRemoteSources enabled="true" /><AppContextSwitchOverrides value="Switch.System.IO.UseLegacyPathHandling=false;Switch.System.IO.BlockLongPaths=false" /></runtime></configuration>'
[IO.File]::WriteAllText((Join-Path $work 'RuntimeProbe.exe.config'),$config,[Text.UTF8Encoding]::new($false))
Add-Type -AssemblyName System.IO.Compression
foreach($item in @(@('runtime-curseforge.zip','manifest.json','runtime-curseforge-manifest.json'),@('runtime-modrinth.mrpack','modrinth.index.json','runtime-modrinth-index.json'))){
 $out=Join-Path $tests $item[0]
 $stream=[IO.File]::Open($out,[IO.FileMode]::Create)
 $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
 try{
  $writer=[IO.StreamWriter]::new($zip.CreateEntry($item[1]).Open(),[Text.UTF8Encoding]::new($false))
  try{$writer.Write([IO.File]::ReadAllText((Join-Path $tests $item[2])))}finally{$writer.Dispose()}
  $writer=[IO.StreamWriter]::new($zip.CreateEntry('overrides/runtime-verification.txt').Open(),[Text.UTF8Encoding]::new($false))
  try{$writer.Write('Test-only fixture for isolated PCL download restoration verification.')}finally{$writer.Dispose()}
 }finally{$zip.Dispose();$stream.Dispose()}
}
$probe=Join-Path $work 'RuntimeProbe.exe'
foreach($mode in @('jit','online')){& $probe $PatchedExe $mode;if($LASTEXITCODE -ne 0){throw ('Probe failed: '+$mode)}}
if(!$SkipDownloads){foreach($mode in @('vanilla','packs')){& $probe $PatchedExe $mode 240;if($LASTEXITCODE -ne 0){throw ('Probe failed: '+$mode)}}}
Write-Output ('Verification logs: '+$work)
