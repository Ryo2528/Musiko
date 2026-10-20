$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$appFile = Join-Path $PSScriptRoot 'index.html'
if (-not (Test-Path -LiteralPath $appFile)) {
    [System.Windows.Forms.MessageBox]::Show("Musiko could not find its player interface:`n$appFile", 'Musiko') | Out-Null
    exit 1
}

$edgeCandidates = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
)
$edge = $edgeCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $edge) {
    $edgeCommand = Get-Command msedge.exe -ErrorAction SilentlyContinue
    if ($edgeCommand) { $edge = $edgeCommand.Source }
}
if (-not $edge) {
    [System.Windows.Forms.MessageBox]::Show('Microsoft Edge was not found. Install or repair Microsoft Edge, then launch Musiko again.', 'Musiko') | Out-Null
    exit 1
}

$appUri = ([System.Uri](Resolve-Path -LiteralPath $appFile).Path).AbsoluteUri
Start-Process -FilePath $edge -ArgumentList @("--app=$appUri", '--new-window')
