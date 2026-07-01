function Show-Error ($message) {
    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show($message, "Build Error", "OK", "Error")
    Exit
}

# Fix path to repo root
$repoPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $repoPath

Write-Host "--- Checking environment before build ---" -ForegroundColor Cyan

if (-not (Get-Command git -ErrorAction SilentlyContinue)) { Show-Error "Git not found! Please run start.bat first." }
if (-not (dotnet --list-sdks 2>$null | Select-String "9.0")) { Show-Error ".NET 9 SDK not found! Run start.bat or restart your PC." }

Write-Host "Pulling updates from GitHub..." -ForegroundColor Yellow
git pull
git submodule update --init --recursive
if ($LASTEXITCODE -ne 0) { Show-Error "Failed to update repository or submodules." }

Write-Host "`nGenerating resources (Python)..." -ForegroundColor Yellow
if (Test-Path ".\RUN_THIS.py") {
    if (-not (Get-Command python -ErrorAction SilentlyContinue)) { Show-Error "Python not found in your system!" }
    py .\RUN_THIS.py
    if ($LASTEXITCODE -ne 0) { Show-Error "Python script RUN_THIS.py failed with error." }
} else {
    Write-Host "[Warning] RUN_THIS.py not found, skipping." -ForegroundColor Yellow
}

Write-Host "`nBuilding the game (.NET 9)..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Show-Error "Compilation failed! Check the console logs above."
}

Write-Host "`n====================================================" -ForegroundColor Green
Write-Host " SUCCESS! The game has been built." -ForegroundColor Green
Write-Host " Launch: bin/Content.Client/Content.Client.exe" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Green

Read-Host "Press Enter to close this window..."
