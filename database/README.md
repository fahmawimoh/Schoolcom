# SchoolCom PostgreSQL Database

## Connection details (local development)

Use these values in **pgAdmin 4**, **backend/.env**, and any SQL client:

| Setting | Value |
|---------|--------|
| **Host** | `localhost` |
| **Port** | `5400` |
| **Database** | `schoolcom` |
| **Username** | `postgres` |
| **Password** | Your password (example: `123456` — set in `backend/.env`) |

**Connection string (URI):**

```
postgresql://postgres:YOUR_PASSWORD@localhost:5400/schoolcom
```

**Npgsql / .env variables:**

```env
DB_HOST=localhost
DB_PORT=5400
DB_NAME=schoolcom
DB_USER=postgres
DB_PASSWORD=YOUR_PASSWORD
DB_SSLMODE=Prefer
```

Optional pooling (defaults shown):

```env
DB_MIN_POOL_SIZE=0
DB_MAX_POOL_SIZE=100
DB_CONNECTION_TIMEOUT=15
DB_COMMAND_TIMEOUT=30
```

---

## Connect with pgAdmin 4

1. Open **pgAdmin 4**.
2. In the left tree, right-click **Servers** → **Register** → **Server…**
3. **General** tab:
   - **Name:** `SchoolCom Local` (any label you prefer)
4. **Connection** tab:
   - **Host name/address:** `localhost`
   - **Port:** `5400`
   - **Maintenance database:** `postgres` (or `schoolcom` after it exists)
   - **Username:** `postgres`
   - **Password:** your PostgreSQL password (e.g. `123456`)
   - Enable **Save password** if you want.
5. Click **Save**.
6. Expand **Servers → SchoolCom Local → Databases → schoolcom → Schemas → public → Tables**.

You should see **22 application tables**, including:

`Tenants`, `TenantSettings`, `Users`, `Roles`, `UserRoleAssignments`, `Students`, `Classrooms`, `Conversations`, `Messages`, `WhatsAppMessages`, `AuditLogs`, and `__EFMigrationsHistory`.

### View sample data in pgAdmin

1. Right-click a table (e.g. **Users**) → **View/Edit Data** → **All Rows**.
2. Or open **Tools → Query Tool** and run:

```sql
SELECT "Email", "FirstName", "LastName" FROM "Users";
SELECT "Name", "Slug" FROM "Tenants";
SELECT "StudentNumber", "FirstName", "LastName" FROM "Students";
```

> PostgreSQL table names are **case-sensitive** when quoted. EF Core creates PascalCase names like `"Users"`.

---

## Start PostgreSQL (Windows)

Ensure the service is running:

```powershell
Get-Service postgresql-x64-18
Start-Service postgresql-x64-18   # if Stopped (run as Administrator)
```

Confirm the port (default install may use **5400** instead of 5432):

```powershell
netstat -ano | findstr 5400
```

---

## Quick setup

From the repository root:

```powershell
.\database\scripts\setup-database.ps1
```

This will:

1. Create the `schoolcom` database (if missing)
2. Apply EF Core migrations (all tables, keys, indexes, constraints)
3. Seed demo tenant, roles, users, academic data, and sample messages

Verify everything:

```powershell
.\database\scripts\verify-connection.ps1
```

---

## Start the backend (uses PostgreSQL automatically)

```powershell
cd backend
# Ensure backend/.env exists (copy from .env.example)
dotnet run --project src/SchoolCom.Api
```

On startup the API will:

1. Load `backend/.env`
2. Connect to PostgreSQL (with connection pooling)
3. Run pending migrations
4. Seed data if the database is empty
5. Expose health checks: `GET http://localhost:5000/health`

---

## Manual setup

### 1. Create database

```powershell
$env:PGPASSWORD = "YOUR_PASSWORD"
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h localhost -p 5400 -U postgres -f database\scripts\00-create-database.sql
```

### 2. Configure environment

```powershell
copy backend\.env.example backend\.env
# Edit backend\.env and set DB_PASSWORD
```

### 3. Apply migrations and seed

```powershell
cd backend
dotnet ef database update --project src\SchoolCom.Infrastructure --startup-project src\SchoolCom.Api
dotnet run --project src\SchoolCom.Tools.Seed --no-launch-profile
```

Pre-generated SQL: `database/scripts/migrations-idempotent.sql`

---

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| School Admin | admin@demo.school | Admin@123456 |
| Parent | parent@demo.school | Admin@123456 |
| Teacher | teacher@demo.school | Admin@123456 |

---

## Tables (22 + migrations history)

Tenants, TenantSettings, Users, Roles, UserRoleAssignments, RefreshTokens, OtpCodes, SchoolBranches, Grades, Sections, Classrooms, Students, ParentStudents, Conversations, ConversationParticipants, Messages, MessageAttachments, MessageModerations, WhatsAppSessions, WhatsAppMessages, AuditLogs, `__EFMigrationsHistory`

Schema is defined in EF Core migration:

`backend/src/SchoolCom.Infrastructure/Persistence/Migrations/20260602130522_InitialPostgresSchema.cs`

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| pgAdmin cannot connect | Check PostgreSQL service is **Running**; confirm port **5400** in server properties |
| `password authentication failed` | Update `DB_PASSWORD` in `backend/.env` to match your `postgres` user password |
| `database "schoolcom" does not exist` | Run `setup-database.ps1` or `00-create-database.sql` |
| API fails at startup | Read console logs; run `verify-connection.ps1` |
| Empty tables in pgAdmin | Run `dotnet run --project src\SchoolCom.Tools.Seed` from `backend` |
