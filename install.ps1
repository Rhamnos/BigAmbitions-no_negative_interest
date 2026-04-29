# No Negative Interest - installer for Windows
$GameName   = "Big Ambitions"
$PluginName = "No Negative Interest"

Write-Host "=== $PluginName installer ===" -ForegroundColor Cyan
Write-Host ""

# Common Steam library locations on Windows
$SearchPaths = @(
    "C:\Program Files (x86)\Steam\steamapps\common\$GameName",
    "C:\Program Files\Steam\steamapps\common\$GameName",
    "$env:PROGRAMFILES\Steam\steamapps\common\$GameName"
)

# Also check Steam's libraryfolders.vdf for extra libraries
$VdfPath = "C:\Program Files (x86)\Steam\steamapps\libraryfolders.vdf"
if (Test-Path $VdfPath) {
    $vdf = Get-Content $VdfPath -Raw
    $paths = [regex]::Matches($vdf, '"path"\s+"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
    foreach ($p in $paths) {
        $SearchPaths += "$p\steamapps\common\$GameName"
    }
}

$GameDir = ""
foreach ($p in $SearchPaths) {
    if (Test-Path "$p\Big Ambitions.exe") {
        $GameDir = $p
        break
    }
}

if (-not $GameDir) {
    Write-Host "Could not auto-detect Big Ambitions install folder."
    Write-Host "Please enter the full path to your Big Ambitions folder"
    Write-Host "(the one containing 'Big Ambitions.exe'):"
    $GameDir = Read-Host
    if (-not (Test-Path "$GameDir\Big Ambitions.exe")) {
        Write-Host "ERROR: Big Ambitions.exe not found at '$GameDir'. Aborting." -ForegroundColor Red
        exit 1
    }
}

Write-Host "Found game at: $GameDir"
Write-Host "Installing $PluginName..."

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Copy-Item -Path "$ScriptDir\payload\*" -Destination $GameDir -Recurse -Force

Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host ""
Write-Host "On Windows, no Steam launch option is needed." -ForegroundColor Yellow
Write-Host "Just launch the game normally."
Write-Host ""
Write-Host "Check BepInEx\LogOutput.log in the game folder to confirm the mod loaded."
