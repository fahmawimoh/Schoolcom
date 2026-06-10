# Clears all application data and re-seeds fresh demo data.
# Schema and migrations are preserved.
param(
    [switch]$StartServer
)

$ErrorActionPreference = "Stop"
$apiDir = Join-Path $PSScriptRoot ".." "backend" "src" "SchoolCom.Api"

Write-Host "Stopping any running SchoolCom.Api process..."
Get-Process -Name "SchoolCom.Api" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

Push-Location $apiDir
try {
    Write-Host "Building backend..."
    dotnet build --no-restore 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        dotnet build
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    }

    Write-Host "Resetting database (truncate all tables + re-seed)..."
    dotnet run --no-build -- --reset-database-only
    if ($LASTEXITCODE -ne 0) { throw "Database reset failed" }

    Write-Host "Database reset complete."

    if ($StartServer) {
        Write-Host "Starting API server..."
        dotnet run --no-build
    }
}
finally {
    Pop-Location
}
