# Verifies PostgreSQL connectivity, schema, seed data, and API health.
# Usage: .\database\scripts\verify-connection.ps1

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
$DbPassword = Get-EnvValue "DB_PASSWORD" ""

if ([string]::IsNullOrWhiteSpace($DbPassword)) {
    throw "DB_PASSWORD not set in backend/.env"
}

Write-Host "=== PostgreSQL connection ===" -ForegroundColor Cyan
Write-Host "Host=$DbHost Port=$DbPort Database=$DbName User=$DbUser Password=***"

$Psql = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
if (-not (Test-Path $Psql)) { $Psql = (Get-Command psql -ErrorAction SilentlyContinue).Source }
if (-not $Psql) { throw "psql not found" }

$env:PGPASSWORD = $DbPassword
& $Psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -c "SELECT version();" | Out-Host
& $Psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -c 'SELECT COUNT(*) AS table_count FROM information_schema.tables WHERE table_schema = ''public'' AND table_type = ''BASE TABLE'';' | Out-Host
$seedCheckSql = Join-Path $env:TEMP "schoolcom-verify.sql"
@'
SELECT COUNT(*) AS users FROM "Users";
SELECT COUNT(*) AS students FROM "Students";
SELECT COUNT(*) AS tenants FROM "Tenants";
'@ | Set-Content -Path $seedCheckSql -Encoding UTF8
& $Psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -f $seedCheckSql | Out-Host
Remove-Item $seedCheckSql -ErrorAction SilentlyContinue

Write-Host "`n=== API health (start API first if this fails) ===" -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -TimeoutSec 5
    $health | ConvertTo-Json | Out-Host
} catch {
    Write-Host "Health endpoint not reachable. Start API: cd backend; dotnet run --project src/SchoolCom.Api" -ForegroundColor Yellow
}

Write-Host "`n=== Auth login (CRUD read/write) ===" -ForegroundColor Cyan
try {
    $body = '{"email":"admin@demo.school","password":"Admin@123456"}'
    $login = Invoke-RestMethod -Uri "http://localhost:5000/v1/auth/login" -Method POST -ContentType "application/json" -Body $body -TimeoutSec 10
    Write-Host "Login OK. User: $($login.data.user.email) Roles: $($login.data.user.roles -join ', ')" -ForegroundColor Green
} catch {
    Write-Host "Login failed: $_" -ForegroundColor Red
}

Write-Host "`nVerification complete." -ForegroundColor Green
