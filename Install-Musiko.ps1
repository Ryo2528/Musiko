$ErrorActionPreference = 'Stop'
$appFile = Join-Path $PSScriptRoot 'index.html'
$launcher = Join-Path $PSScriptRoot 'Run-Musiko.vbs'
if (-not (Test-Path -LiteralPath $appFile) -or -not (Test-Path -LiteralPath $launcher)) {
    throw 'Musiko files are incomplete. Keep Install-Musiko.ps1 beside index.html and Run-Musiko.vbs.'
}

$edgeCandidates = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
)
$edge = $edgeCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $edge) { throw 'Microsoft Edge was not found. Install or repair Edge, then try again.' }

$desktop = [Environment]::GetFolderPath('DesktopDirectory')
$programs = Join-Path ([Environment]::GetFolderPath('Programs')) 'Musiko'
New-Item -ItemType Directory -Path $programs -Force | Out-Null
$shell = New-Object -ComObject WScript.Shell
foreach ($shortcutPath in @((Join-Path $desktop 'Musiko.lnk'), (Join-Path $programs 'Musiko.lnk'))) {
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = Join-Path $env:WINDIR 'System32\wscript.exe'
    $shortcut.Arguments = '"' + $launcher + '"'
    $shortcut.WorkingDirectory = $PSScriptRoot
    $shortcut.Description = 'Open the Musiko music player'
    $shortcut.IconLocation = "$edge,0"
    $shortcut.Save()
}
Write-Host 'Musiko shortcuts created on your Desktop and in the Start Menu.'
