# SchoolCom

Enterprise-grade **School Parent-Teacher Communication Platform** with admin-moderated messaging, bilingual support (Arabic/English), and admin-only WhatsApp integration.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS, Shadcn/UI, next-intl |
| Backend | ASP.NET Core 10, Clean Architecture, CQRS, MediatR, EF Core |
| Database | PostgreSQL 16, Redis 7 |
| Real-time | SignalR (Phase 2) |
| Mobile | React Native / Expo (Phase 3) |

## Quick Start

### Prerequisites

- Node.js 22+
- .NET 10 SDK (for backend)
- Docker & Docker Compose (recommended)

### Option 1: Docker Compose (Full Stack)

```bash
docker compose up --build
```

- Frontend: http://localhost:3000
- API + Swagger: http://localhost:5000/swagger
- PostgreSQL: localhost:5400 (or 5432 with Docker Compose)
- Redis: localhost:6379

### Option 2: Local Development

**Infrastructure:**

```bash
docker compose up postgres redis -d
```

**Backend (PostgreSQL on port 5400):**

1. Copy `backend/.env.example` → `backend/.env` and set `DB_PASSWORD`.
2. Create DB + schema + seed (first time only):

```powershell
.\database\scripts\setup-database.ps1
```

3. Start the API:

```powershell
cd backend
dotnet run --project src/SchoolCom.Api
```

4. Verify: `GET http://localhost:5000/health` and `.\database\scripts\verify-connection.ps1`

See **[database/README.md](database/README.md)** for pgAdmin 4 connection steps and full PostgreSQL documentation.

**Frontend:**

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:3000/en/login

## Demo Credentials

| Role | Email | Password | Phone (OTP) |
|------|-------|----------|-------------|
| School Admin | admin@demo.school | Admin@123456 | +966501234567 |
| Parent | parent@demo.school | Admin@123456 | +966509876543 |
| Teacher | teacher@demo.school | Admin@123456 | +966507654321 |

> OTP codes are printed to the API console in development mode.

## Key Features (Phase 1)

- Secure login via **email/password** or **phone OTP**
- **Bilingual UI** (English / Arabic) with RTL layout support
- **Dark / Light mode**
- Role-based dashboards (Admin, Teacher, Parent)
- **Admin-only WhatsApp** messaging module
- Clean Architecture backend with JWT auth, rate limiting, audit logs

## Project Structure

See [docs/05-folder-structure.md](docs/05-folder-structure.md) for complete layout.

## Documentation

| Document | Description |
|----------|-------------|
| [System Architecture](docs/01-system-architecture.md) | High-level design, multi-tenancy, security |
| [Database Design](docs/02-database-design.md) | ERD, tables, indexing |
| [API Design](docs/03-api-design.md) | REST endpoints, SignalR hubs |
| [User Stories & Roadmap](docs/04-user-stories-roadmap.md) | Requirements, phases, deployment |

## Environment Variables

### Frontend (`frontend/.env.local`)

```
NEXT_PUBLIC_API_URL=http://localhost:5000/v1
```

### Backend (`backend/.env`)

```env
DB_HOST=localhost
DB_PORT=5400
DB_NAME=schoolcom
DB_USER=postgres
DB_PASSWORD=your_password_here
```

The API loads `backend/.env` automatically. Migrations and seed data run on startup.

| pgAdmin / client | Value |
|------------------|--------|
| Host | `localhost` |
| Port | `5400` |
| Database | `schoolcom` |
| Username | `postgres` |
| Password | Same as `DB_PASSWORD` in `.env` |

Also configure JWT secret and CORS in `backend/src/SchoolCom.Api/appsettings.json`.

## Security Notes

- Change JWT secret before production deployment
- Enable MFA for admin accounts (Phase 2)
- Configure WhatsApp Business API with Meta developer credentials
- Use HTTPS and secure cookie settings in production

## License

Proprietary — All rights reserved.
