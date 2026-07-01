function Show-Error ($message) {
    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show($message, "Ошибка сборки", "OK", "Error")
    Exit
}

# Фиксируем путь к папке, где лежит сам скрипт (корень репы)
$repoPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $repoPath

Write-Host "--- Проверка окружения перед сборкой ---" -ForegroundColor Cyan

if (-not (Get-Command git -ErrorAction SilentlyContinue)) { Show-Error "Git не найден в системе! Запусти сначала start.bat" }
if (-not (dotnet --list-sdks 2>$null | Select-String "9.0")) { Show-Error ".NET 9 SDK не найден в системе! Запусти сначала start.bat или перезагрузи ПК." }

Write-Host "Подтягиваем изменения из GitHub..." -ForegroundColor Yellow
git pull
git submodule update --init --recursive
if ($LASTEXITCODE -ne 0) { Show-Error "Ошибка при обновлении репозитория или субмодулей." }

Write-Host "`nГенерация ресурсов (Python)..." -ForegroundColor Yellow
if (Test-Path ".\RUN_THIS.py") {
    # Проверяем, установлен ли python
    if (-not (Get-Command python -ErrorAction SilentlyContinue)) { Show-Error "Python не найден в системе!" }
    py .\RUN_THIS.py
    if ($LASTEXITCODE -ne 0) { Show-Error "Скрипт предсборки RUN_THIS.py завершился с ошибкой." }
} else {
    Write-Host "[Предупреждение] RUN_THIS.py не найден, пропускаем." -ForegroundColor Windows
}

Write-Host "`nКомпиляция движка и игры (.NET 9)..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Show-Error "Критическая ошибка компиляции проекта! Сборка упала. Проверь логи выше."
}

Write-Host "`n====================================================" -ForegroundColor Green
Write-Host " ВСЁ ГОТОВО! Игра успешно собрана." -ForegroundColor Green
Write-Host " Запуск: bin/Content.Client/Content.Client.exe" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Green

Read-Host "Нажми Enter, чтобы закрыть окно..."
