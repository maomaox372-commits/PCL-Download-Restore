param([string]$Installer=(Join-Path $PSScriptRoot '../artifacts/PCL.DownloadRestore.exe'))
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$framework=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
if(-not(Test-Path -LiteralPath (Join-Path $framework 'csc.exe'))){$framework=Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319'}
$out=Join-Path $root 'artifacts/test'
[void][IO.Directory]::CreateDirectory($out)
$cecil=Join-Path $root 'lib/Mono.Cecil.dll'
Copy-Item -LiteralPath $cecil -Destination (Join-Path $out 'Mono.Cecil.dll') -Force
$exe=Join-Path $out 'StartupNoticeTests.exe'
& (Join-Path $framework 'csc.exe') /nologo /codepage:65001 /target:exe ('/out:'+$exe) ('/r:'+$cecil) /r:System.Core.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll (Join-Path $PSScriptRoot 'StartupNoticeTests.cs')
if($LASTEXITCODE -ne 0){throw 'Startup notice test compilation failed.'}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'TestHost.config') -Destination ($exe+'.config') -Force
$start=New-Object Diagnostics.ProcessStartInfo
$start.FileName=$exe
$start.Arguments='"'+[IO.Path]::GetFullPath($Installer)+'" "'+(Join-Path $out 'startup-notice-preview.png')+'"'
$start.UseShellExecute=$false
$start.CreateNoWindow=$true
$start.RedirectStandardOutput=$true
$start.RedirectStandardError=$true
$p=New-Object Diagnostics.Process
$p.StartInfo=$start
try{
 [void]$p.Start();$stdout=$p.StandardOutput.ReadToEndAsync();$stderr=$p.StandardError.ReadToEndAsync()
 if(-not $p.WaitForExit(30000)){$p.Kill();throw 'Startup notice tests timed out.'}
 Write-Output $stdout.GetAwaiter().GetResult()
 $errorText=$stderr.GetAwaiter().GetResult()
 if($p.ExitCode -ne 0){throw ('Startup notice tests failed: '+$errorText)}
}finally{$p.Dispose()}
