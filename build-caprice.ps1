$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$capriceDir = Join-Path $root 'caprice'
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$compiler = 'C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\Roslyn\csc.exe'

if (-not (Test-Path -LiteralPath $compiler)) {
    throw 'csc.exe not found in Microsoft.NET\Framework64\v4.0.30319'
}

$harmony = Join-Path $capriceDir '0Harmony.dll'
if (-not (Test-Path -LiteralPath $harmony)) {
    throw '0Harmony.dll not found in caprice folder.'
}

# Définition et création du dossier de sortie out/
$outDir = Join-Path $capriceDir 'out'
if (-not (Test-Path -LiteralPath $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

# Emplacement final de l'exécutable dans out/
$outputExe = Join-Path $outDir 'CaprICE.exe'

$sources = @(
    (Join-Path $capriceDir 'NeonTheme.cs'),
    (Join-Path $capriceDir 'SecurityPolicy.cs'),
    (Join-Path $capriceDir 'CaprICEHook.cs'),
    (Join-Path $capriceDir 'CaprICEForm.cs'),
    (Join-Path $capriceDir 'MemoryManager.cs'),
    (Join-Path $capriceDir 'Program.cs')
)

$args = @(
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

Write-Host "Compiling CaprICE.exe to $outDir..." -ForegroundColor Cyan
& $compiler $args

if ($LASTEXITCODE -ne 0) {
    throw "Compilation of CaprICE failed."
}

# Copie de 0Harmony.dll dans le dossier out/ pour assurer le bon fonctionnement de l'exécutable
Copy-Item -Path $harmony -Destination (Join-Path $outDir '0Harmony.dll') -Force

Write-Host "Successfully built: $outputExe" -ForegroundColor Green