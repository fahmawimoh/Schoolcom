# SchoolCom — System Architecture

## Executive Summary

SchoolCom is a multi-tenant SaaS platform enabling secure parent–teacher communication with mandatory admin moderation, real-time messaging, and enterprise-grade security. The architecture follows Clean Architecture on the backend, feature-sliced design on the frontend, and shared domain logic between web and mobile.

---

## High-Level Architecture

```mermaid
flowchart TB
    subgraph Clients
        WEB[Next.js Web App]
        MOB[React Native / Expo]
    end

    subgraph Edge
        CDN[Cloudflare CDN]
        WAF[WAF / Rate Limiter]
    end

    subgraph API["ASP.NET Core 10 API"]
        GW[API Gateway / BFF]
        AUTH[Auth Service]
        MSG[Messaging Service]
        MOD[Moderation Service]
        NOTIF[Notification Service]
        WA[WhatsApp Admin Service]
    end

    subgraph RealTime
        HUB[SignalR Hub]
    end

    subgraph Data
        PG[(PostgreSQL)]
        REDIS[(Redis)]
        ES[(Elasticsearch)]
        S3[Object Storage S3/R2]
    end

    subgraph External
        FCM[Firebase FCM]
        TWILIO[Twilio SMS]
        SMTP[Email SMTP]
        META[WhatsApp Business API]
        AI[AI Moderation APIs]
    end

    WEB --> CDN --> WAF --> GW
    MOB --> WAF --> GW
    GW --> AUTH & MSG & MOD & NOTIF & WA
    MSG --> HUB
    GW --> PG & REDIS
    MSG --> ES
    MOD --> S3
    NOTIF --> FCM & TWILIO & SMTP
    WA --> META
    MOD --> AI
```

---

## Multi-Tenancy Model

| Strategy | Implementation |
|----------|----------------|
| **Isolation** | Row-level `TenantId` (SchoolId) on all tenant-scoped entities |
| **Routing** | Subdomain (`school-a.schoolcom.com`) or custom domain |
| **Branding** | Per-tenant theme config stored in `TenantSettings` |
| **Super Admin** | Cross-tenant access via explicit policy + audit trail |

**Recommendation:** Shared database with tenant discriminator (PostgreSQL RLS optional for defense-in-depth). Scales to 100K+ users with proper indexing on `(TenantId, ...)`.

---

## Database Architecture Recommendation

### Primary: PostgreSQL 16+

| Pros | Cons |
|------|------|
| ACID, JSONB for flexible metadata | Vertical scaling limits vs NoSQL |
| Excellent multi-tenant indexing | Requires DBA expertise at scale |
| Full-text search (pg_trgm) | Elasticsearch needed for advanced message search |
| Mature ecosystem, EF Core support | |

### Cache: Redis 7+

| Pros | Cons |
|------|------|
| Session, OTP, rate limiting, SignalR backplane | Memory cost |
| Sub-ms reads for hot data | Persistence configuration needed |

### Search: Elasticsearch (Optional Phase 2)

| Pros | Cons |
|------|------|
| Full-text message search at scale | Operational complexity |
| Analytics aggregations | Sync pipeline required |

### Object Storage

| Provider | Pros | Cons |
|----------|------|------|
| **AWS S3** | Industry standard, lifecycle policies | Cost at scale |
| **Cloudflare R2** | No egress fees, S3-compatible API | Smaller ecosystem |
| **Azure Blob** | Azure integration | Vendor lock-in |

**Recommendation:** PostgreSQL + Redis + Cloudflare R2 (cost-effective media) + Elasticsearch in Phase 2.

---

## Backend Clean Architecture Layers

```
SchoolCom.Api          → Controllers, SignalR Hubs, Middleware, DI
SchoolCom.Application  → CQRS (Commands/Queries), DTOs, Validators, Interfaces
SchoolCom.Domain       → Entities, Value Objects, Domain Events, Enums
SchoolCom.Infrastructure → EF Core, Redis, S3, External APIs, Identity
```

**Dependency Rule:** Domain ← Application ← Infrastructure ← Api

---

## Messaging Flow (Parent → Admin → Teacher)

```mermaid
sequenceDiagram
    participant P as Parent
    participant API as API
    participant MOD as Moderation Queue
    participant A as School Admin
    participant T as Teacher
    participant HUB as SignalR

    P->>API: Send message + attachments
    API->>API: Store as PendingModeration
    API->>MOD: Enqueue for review
    API->>HUB: Notify Admin (pending count)
    A->>API: Approve / Reject / Request Changes
    alt Approved
        API->>API: Status = Approved, visible to Teacher
        API->>HUB: Deliver to Teacher thread
        API->>HUB: Notify Parent (approved)
    else Rejected
        API->>HUB: Notify Parent (rejected + reason)
    end
    T->>API: Reply (may require moderation per school policy)
    API->>HUB: Real-time delivery
```

---

## Authentication Flow

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend
    participant API as Auth API
    participant REDIS as Redis

    alt Email + Password
        U->>FE: Enter credentials
        FE->>API: POST /auth/login
        API->>API: Validate + MFA check
        API->>FE: Access JWT + Refresh Token (httpOnly cookie)
    else Phone OTP
        U->>FE: Enter phone
        FE->>API: POST /auth/otp/request
        API->>REDIS: Store OTP (5 min TTL)
        API->>U: SMS OTP
        U->>FE: Enter OTP
        FE->>API: POST /auth/otp/verify
        API->>FE: JWT + Refresh Token
    end
    FE->>API: API calls with Bearer token
    Note over FE,API: Refresh via POST /auth/refresh (rotation)
```

---

## Security Architecture

| Layer | Control |
|-------|---------|
| Transport | TLS 1.3, HSTS |
| Auth | JWT (15 min), Refresh rotation (7 days), MFA (TOTP) |
| AuthZ | RBAC + Policy-based (tenant scope) |
| Input | FluentValidation, parameterized queries |
| XSS | CSP headers, output encoding, React auto-escape |
| CSRF | SameSite cookies + anti-forgery for cookie auth |
| Rate Limit | Redis sliding window per IP/user |
| Audit | Immutable `AuditLog` table, Serilog structured logs |
| Media | Pre-signed URLs, virus scan hook, size/type validation |

---

## Scalability Targets

| Metric | Target |
|--------|--------|
| Concurrent users | 10,000+ per region |
| Total users | 100,000+ |
| Message throughput | 1,000 msg/sec (horizontal SignalR scale-out) |
| Availability | 99.9% SLA |

**Scale-out:** Stateless API pods, Redis SignalR backplane, read replicas for PostgreSQL, CDN for static/media.

---

## DevOps Architecture

```mermaid
flowchart LR
    GH[GitHub Actions] --> BUILD[Build & Test]
    BUILD --> DOCKER[Docker Images]
    DOCKER --> REG[Container Registry]
    REG --> K8S[Kubernetes]
    K8S --> OTEL[OpenTelemetry]
    OTEL --> PROM[Prometheus]
    PROM --> GRAF[Grafana]
```

---

## Technology Version Matrix

| Component | Version |
|-----------|---------|
| ASP.NET Core | 10.x |
| Next.js | 16.x |
| React | 20.x |
| PostgreSQL | 16+ |
| Redis | 7+ |
| Node.js | 22 LTS |
