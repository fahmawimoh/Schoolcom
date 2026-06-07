# SchoolCom — Folder Structure

## Repository Layout

```
school-com/
├── docs/                          # Architecture & design documentation
│   ├── 01-system-architecture.md
│   ├── 02-database-design.md
│   ├── 03-api-design.md
│   ├── 04-user-stories-roadmap.md
│   └── 05-folder-structure.md
├── backend/                       # ASP.NET Core 10 Clean Architecture
│   ├── SchoolCom.sln
│   ├── Dockerfile
│   └── src/
│       ├── SchoolCom.Domain/      # Entities, Enums, Value Objects
│       ├── SchoolCom.Application/ # CQRS, Validators, Interfaces
│       ├── SchoolCom.Infrastructure/ # EF Core, Redis, External APIs
│       └── SchoolCom.Api/         # REST Controllers, Program.cs
├── frontend/                      # Next.js 16 Web Application
│   ├── messages/                  # i18n translations (en, ar)
│   ├── src/
│   │   ├── app/[locale]/          # App Router with locale segment
│   │   ├── components/            # UI & feature components
│   │   ├── i18n/                  # next-intl configuration
│   │   ├── lib/                   # API client, auth, validations
│   │   └── providers/             # React Query, theme
│   └── Dockerfile
├── mobile/                        # React Native / Expo (Phase 3)
├── docker-compose.yml
├── .github/workflows/
└── README.md
```

## Backend Clean Architecture

```
SchoolCom.Domain/
├── Common/           BaseEntity, TenantEntity
├── Entities/         User, Student, Message, Tenant, ...
└── Enums/            UserRole, ModerationStatus, MessageType

SchoolCom.Application/
├── Common/
│   ├── Behaviors/    ValidationBehavior (MediatR pipeline)
│   └── Interfaces/   IJwtTokenService, IOtpService, IWhatsAppService
└── Features/
    ├── Auth/Commands/Login, RequestOtp, VerifyOtp
    └── WhatsApp/Commands/SendWhatsAppMessage

SchoolCom.Infrastructure/
├── Identity/         JWT, CurrentUser, BCrypt
├── Persistence/      EF Core DbContext, Repositories, Seeder
└── Services/         OTP, WhatsApp, Audit

SchoolCom.Api/
├── Controllers/v1/   AuthController, WhatsAppController
├── Models/           ApiResponse<T>
└── Program.cs        DI, Auth, Swagger, CORS, Rate Limiting
```

## Next.js 16 Frontend Structure

```
src/app/[locale]/
├── layout.tsx              Root locale layout (RTL/LTR, providers)
├── page.tsx                Redirect to login
├── login/page.tsx          Secure login entry point
├── admin/
│   ├── page.tsx            School Admin dashboard
│   └── whatsapp/page.tsx   Admin-only WhatsApp integration
├── teacher/page.tsx        Teacher dashboard
└── parent/page.tsx         Parent dashboard

src/components/
├── auth/                   LoginForm, LoginPageContent
├── layout/                 DashboardShell, ThemeToggle, LanguageSwitcher
└── ui/                     Shadcn-style primitives (Button, Input, Card)

src/lib/
├── api/client.ts           REST API client
├── auth/store.ts           Zustand auth state
└── validations/auth.ts     Zod schemas
```

## React Native Structure (Planned)

```
mobile/
├── app/                    Expo Router screens
├── src/
│   ├── features/           auth, messages, notifications
│   ├── shared/             API client (shared with web)
│   └── i18n/               Shared translations
└── app.json
```
