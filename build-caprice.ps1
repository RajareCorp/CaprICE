<#
    Compile CaprICE.exe depuis src/ (récursif) vers out/.

    Usage :  powershell -ExecutionPolicy Bypass -File caprice\build-caprice.ps1
             (optionnel)  -Compiler "C:\chemin\vers\csc.exe"

    Prérequis : 0Harmony.dll dans lib\ (ou, à défaut, à la racine du dossier caprice\).
#>
param(
    [string]$Compiler
)

$ErrorActionPreference = 'Stop'
$capriceDir = $PSScriptRoot

# ---------- Compilateur C# ----------
if (-not $Compiler) {
    $candidates = @(
        'C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\Roslyn\csc.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin\Roslyn\csc.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Msbuild\Current\Bin\Roslyn\csc.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\BuildTools\Msbuild\Current\Bin\Roslyn\csc.exe',
        (Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe')
    )
    $Compiler = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $Compiler -or -not (Test-Path -LiteralPath $Compiler)) {
    throw 'csc.exe introuvable. Installez Visual Studio Build Tools ou passez -Compiler "<chemin>\csc.exe".'
}

# ---------- Harmony ----------
$harmony = @((Join-Path $capriceDir 'lib\0Harmony.dll'), (Join-Path $capriceDir '0Harmony.dll')) |
    Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $harmony) {
    throw '0Harmony.dll introuvable (attendu dans caprice\lib\ ou caprice\).'
}

# ---------- Sources (tous les .cs de src\, récursif) ----------
$srcDir = Join-Path $capriceDir 'src'
$sources = Get-ChildItem -LiteralPath $srcDir -Recurse -Filter '*.cs' | ForEach-Object { $_.FullName }
if (-not $sources) { throw "Aucun fichier .cs trouvé dans $srcDir" }

# ---------- Sortie ----------
$outDir = Join-Path $capriceDir 'out'
if (-not (Test-Path -LiteralPath $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}
$outputExe = Join-Path $outDir 'CaprICE.exe'

$cscArgs = @(
    '/nologo',
    '/target:winexe',
    '/platform:anycpu',
    '/optimize+',
    '/utf8output',
    '/warn:0',
    '/reference:System.dll',
    '/reference:System.Core.dll',
    '/reference:System.Drawing.dll',
    '/reference:System.Windows.Forms.dll',
    ('/reference:' + $harmony),
    ('/out:' + $outputExe)
) + $sources

Write-Host "Compiling CaprICE.exe ($($sources.Count) files) to $outDir..." -ForegroundColor Cyan
& $Compiler $cscArgs

if ($LASTEXITCODE -ne 0) {
    throw 'Compilation of CaprICE failed.'
}

# 0Harmony.dll doit accompagner l'exécutable
Copy-Item -Path $harmony -Destination (Join-Path $outDir '0Harmony.dll') -Force

Write-Host "Successfully built: $outputExe" -ForegroundColor Green
