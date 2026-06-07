# SchoolCom PostgreSQL setup — creates DB, applies migrations, seeds data.
# Usage: .\database\scripts\setup-database.ps1

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$Backend = Join-Path $RepoRoot "backend"
$EnvFile = Join-Path $Backend ".env"

function Get-EnvValue([string]$Name, [string]$Default) {
    if (Test-Path $EnvFile) {
        foreach ($line in Get-Content $EnvFile) {
            if ($line -match "^\s*$Name\s*=\s*(.+)\s*$") { return $Matches[1].Trim() }
        }
    }
    return $Default
}

$DbHost = Get-EnvValue "DB_HOST" "localhost"
$DbPort = Get-EnvValue "DB_PORT" "5400"
$DbName = Get-EnvValue "DB_NAME" "schoolcom"
$DbUser = Get-EnvValue "DB_USER" "postgres"
$DbPassword = Get-EnvValue "DB_PASSWORD" "123456"

$Psql = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
if (-not (Test-Path $Psql)) {
    $Psql = (Get-Command psql -ErrorAction SilentlyContinue)?.Source
}
if (-not $Psql) { throw "psql not found. Install PostgreSQL or set path." }

$env:PGPASSWORD = $DbPassword
Write-Host "Creating database '$DbName' on ${DbHost}:${DbPort}..." -ForegroundColor Cyan
& $Psql -h $DbHost -p $DbPort -U $DbUser -d postgres -f (Join-Path $PSScriptRoot "00-create-database.sql")

Write-Host "Applying EF Core migrations..." -ForegroundColor Cyan
Push-Location $Backend
try {
    $env:DB_HOST = $DbHost
    $env:DB_PORT = $DbPort
    $env:DB_NAME = $DbName
    $env:DB_USER = $DbUser
    $env:DB_PASSWORD = $DbPassword

    dotnet ef database update `
        --project src\SchoolCom.Infrastructure `
        --startup-project src\SchoolCom.Api
}
finally {
    Pop-Location
}

Write-Host "Seeding database..." -ForegroundColor Cyan
Push-Location $Backend
try {
    dotnet run --project src\SchoolCom.Tools.Seed --no-launch-profile
}
finally {
    Pop-Location
}

Write-Host "Done. Database '$DbName' is ready." -ForegroundColor Green
Write-Host "Demo login: admin@demo.school / Admin@123456"
