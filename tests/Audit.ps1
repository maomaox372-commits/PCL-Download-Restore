param(
 [string]$Original = (Join-Path $PSScriptRoot 'fixture\PCL.exe'),
 [string]$Patched = (Join-Path $PSScriptRoot 'patched\PCL.exe')
)
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot '..\artifacts\Mono.Cecil.dll')
$before = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($Original)
$after = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($Patched)
function Get-Methods($types) { foreach($type in $types) { $type.Methods; Get-Methods $type.NestedTypes } }
function Get-Signature($method) {
 if(-not $method.HasBody) { return '' }
 $code = $method.Body.Instructions
 (($code | ForEach-Object {
  $op = $_.OpCode.Name -replace '\.s$',''
  $operand = $_.Operand
  if($operand -is [Mono.Cecil.Cil.Instruction]) { $operand = $code.IndexOf($operand) }
  elseif($operand -is [Mono.Cecil.Cil.Instruction[]]) { $operand = ($operand | ForEach-Object { $code.IndexOf($_) }) -join ',' }
  $op + ' ' + [string]$operand
 }) -join "`n")
}
$originalMethods = @{}
Get-Methods $before.MainModule.Types | ForEach-Object { $originalMethods[$_.FullName] = Get-Signature $_ }
$changed = @(Get-Methods $after.MainModule.Types | Where-Object { (Get-Signature $_) -ne $originalMethods[$_.FullName] } | ForEach-Object FullName)
$expected = @(
 'System.Void PCL.FormMain::FormMain_Loaded(System.Object,System.Windows.RoutedEventArgs)',
 'System.Void PCL.ModModpack::ModpackInstall()',
 'PCL.ModLoader/LoaderCombo`1<System.String> PCL.ModModpack::ModpackInstall(System.String,System.String,System.String)',
 'System.Void PCL.PageDownloadInstall::LoaderInit()',
 'System.Void PCL.PageDownloadModify::LoadInstanceFinished(PCL.ModLoader/LoaderTask`2<PCL.ModMinecraft/McInstance,System.Object>)',
 'System.Void PCL.PageDownloadResourceDetail::InitializeStackResourceVersions(System.Windows.Controls.StackPanel)'
)
if(Compare-Object ($changed | Sort-Object) ($expected | Sort-Object)) { throw 'Changed method set is not exactly the expected six methods.' }
if($originalMethods.Count -ne @(Get-Methods $after.MainModule.Types).Count) { throw 'Method count changed.' }
if(Compare-Object @($before.MainModule.AssemblyReferences.FullName) @($after.MainModule.AssemblyReferences.FullName)) { throw 'Assembly references changed.' }
$resources = @()
foreach($res in $before.MainModule.Resources) {
 $newRes = $after.MainModule.Resources | Where-Object Name -eq $res.Name
 if($res -is [Mono.Cecil.EmbeddedResource]) {
  $sha = [Security.Cryptography.SHA256]::Create()
  try {
   $a = [BitConverter]::ToString($sha.ComputeHash($res.GetResourceData()))
   $b = [BitConverter]::ToString($sha.ComputeHash($newRes.GetResourceData()))
   if($a -ne $b) { throw ('Embedded resource changed: ' + $res.Name) }
   $resources += $res.Name
  } finally { $sha.Dispose() }
 }
}
$report = [ordered]@{ result='PASS'; originalSha256=(Get-FileHash -LiteralPath $Original).Hash; patchedSha256=(Get-FileHash -LiteralPath $Patched).Hash; methodsTotal=$originalMethods.Count; changedMethods=$changed; unchangedMethods=$originalMethods.Count-$changed.Count; resourcesUnchanged=$resources; assemblyReferencesUnchanged=$true }
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'audit-result.json') -Encoding UTF8
$report | ConvertTo-Json -Depth 4
$before.Dispose(); $after.Dispose()
