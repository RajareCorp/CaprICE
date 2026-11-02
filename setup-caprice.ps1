[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param(
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'

$repository = 'RajareCorp/CaprICE'
$archiveName = 'CaprICE-windows.zip'
$checksumName = "$archiveName.sha256"
$installDirectory = Join-Path $env:LOCALAPPDATA 'Programs\CaprICE'
$programsDirectory = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
$shortcutDirectory = Join-Path $programsDirectory 'CaprICE'
$shortcutPath = Join-Path $shortcutDirectory 'CaprICE.lnk'

if ($Uninstall) {
    if ($PSCmdlet.ShouldProcess($installDirectory, 'Uninstall CaprICE')) {
        if (Get-Process -Name 'CaprICE' -ErrorAction SilentlyContinue) {
            throw 'Close CaprICE before uninstalling it.'
        }

        foreach ($fileName in @('CaprICE.exe', '0Harmony.dll')) {
            $filePath = Join-Path $installDirectory $fileName
            if (Test-Path -LiteralPath $filePath) {
                Remove-Item -LiteralPath $filePath -Force
            }
        }

        if (Test-Path -LiteralPath $shortcutPath) {
            Remove-Item -LiteralPath $shortcutPath -Force
        }

        foreach ($directory in @($shortcutDirectory, $installDirectory)) {
            if ((Test-Path -LiteralPath $directory) -and
                (Get-ChildItem -LiteralPath $directory -Force | Measure-Object).Count -eq 0) {
                Remove-Item -LiteralPath $directory -Force
            }
        }

        Write-Host 'CaprICE has been uninstalled. User data was not removed.' -ForegroundColor Green
    }
    return
}

if (-not $PSCmdlet.ShouldProcess($installDirectory, 'Install or update CaprICE')) {
    return
}

if (Get-Process -Name 'CaprICE' -ErrorAction SilentlyContinue) {
    throw 'Close CaprICE before installing or updating it.'
}

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$headers = @{
    'User-Agent' = 'CaprICE-Setup'
    'Accept'     = 'application/vnd.github+json'
}
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repository/releases/latest" -Headers $headers
$archiveAsset = $release.assets | Where-Object { $_.name -eq $archiveName } | Select-Object -First 1
$checksumAsset = $release.assets | Where-Object { $_.name -eq $checksumName } | Select-Object -First 1
if (-not $archiveAsset -or -not $checksumAsset) {
    throw "The latest GitHub release does not contain $archiveName and $checksumName."
}

$programsParent = Split-Path -Parent $installDirectory
$stagingDirectory = Join-Path $programsParent ('.CaprICE-stage-' + [Guid]::NewGuid().ToString('N'))
$backupDirectory = Join-Path $programsParent ('.CaprICE-backup-' + [Guid]::NewGuid().ToString('N'))
$downloadDirectory = Join-Path ([IO.Path]::GetTempPath()) ('CaprICE-setup-' + [Guid]::NewGuid().ToString('N'))
$archivePath = Join-Path $downloadDirectory $archiveName
$checksumPath = Join-Path $downloadDirectory $checksumName
$backupCreated = $false

try {
    New-Item -ItemType Directory -Path $programsParent -Force | Out-Null
    New-Item -ItemType Directory -Path $stagingDirectory -Force | Out-Null
    New-Item -ItemType Directory -Path $downloadDirectory -Force | Out-Null

    Invoke-WebRequest -Uri $archiveAsset.browser_download_url -Headers $headers -OutFile $archivePath -UseBasicParsing
    Invoke-WebRequest -Uri $checksumAsset.browser_download_url -Headers $headers -OutFile $checksumPath -UseBasicParsing

    $checksumText = [IO.File]::ReadAllText($checksumPath).Trim()
    if ($checksumText -notmatch '^(?<hash>[0-9a-fA-F]{64})\s+CaprICE-windows\.zip$') {
        throw 'The release checksum file has an invalid format.'
    }
    $expectedHash = $Matches.hash.ToUpperInvariant()
    $actualHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
    if ($actualHash -ne $expectedHash) {
        throw 'The downloaded archive failed its SHA-256 integrity check.'
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $requiredFiles = @('CaprICE.exe', '0Harmony.dll')
        if ($archive.Entries.Count -ne $requiredFiles.Count) {
            throw 'The release archive contains an unexpected set of files.'
        }

        foreach ($entry in $archive.Entries) {
            if ($entry.FullName -notin $requiredFiles) {
                throw "Unexpected file in release archive: $($entry.FullName)"
            }

            $destination = Join-Path $stagingDirectory $entry.FullName
            $sourceStream = $entry.Open()
            try {
                $destinationStream = [IO.File]::Create($destination)
                try {
                    $sourceStream.CopyTo($destinationStream)
                }
                finally {
                    $destinationStream.Dispose()
                }
            }
            finally {
                $sourceStream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
    }

    foreach ($fileName in @('CaprICE.exe', '0Harmony.dll')) {
        $filePath = Join-Path $stagingDirectory $fileName
        if (-not (Test-Path -LiteralPath $filePath) -or (Get-Item -LiteralPath $filePath).Length -eq 0) {
            throw "The release archive is missing a valid $fileName."
        }
    }

    if (Test-Path -LiteralPath $installDirectory) {
        Move-Item -LiteralPath $installDirectory -Destination $backupDirectory
        $backupCreated = $true
    }

    try {
        Move-Item -LiteralPath $stagingDirectory -Destination $installDirectory
    }
    catch {
        if ($backupCreated -and -not (Test-Path -LiteralPath $installDirectory)) {
            Move-Item -LiteralPath $backupDirectory -Destination $installDirectory
            $backupCreated = $false
        }
        throw
    }

    if ($backupCreated) {
        Remove-Item -LiteralPath $backupDirectory -Recurse -Force
        $backupCreated = $false
    }

    New-Item -ItemType Directory -Path $shortcutDirectory -Force | Out-Null
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = Join-Path $installDirectory 'CaprICE.exe'
    $shortcut.WorkingDirectory = $installDirectory
    $shortcut.IconLocation = Join-Path $installDirectory 'CaprICE.exe'
    $shortcut.Save()

    Write-Host "CaprICE $($release.tag_name) installed in $installDirectory." -ForegroundColor Green
    Write-Host 'Launch it from the Start Menu. Capri Desktop is not included.'
}
finally {
    if (Test-Path -LiteralPath $stagingDirectory) {
        Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
    }
    if (Test-Path -LiteralPath $downloadDirectory) {
        Remove-Item -LiteralPath $downloadDirectory -Recurse -Force
    }
}
