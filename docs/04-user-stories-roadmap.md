# User Stories & Requirements

## User Stories

### Super Admin

- As a Super Admin, I want to onboard new schools so that they can use the platform independently.
- As a Super Admin, I want to view cross-tenant analytics so that I can monitor platform health.
- As a Super Admin, I want to configure global subscription plans so that schools have appropriate limits.

### School Admin

- As a School Admin, I want to review parent messages before teachers see them so that inappropriate content is filtered.
- As a School Admin, I want to approve/reject media attachments so that teachers only see vetted content.
- As a School Admin, I want to communicate with parents via WhatsApp so that I can reach them on their preferred channel.
- As a School Admin, I want to manage teachers, parents, and students so that the school roster is accurate.
- As a School Admin, I want to configure moderation rules so that teacher replies can skip or require review.
- As a School Admin, I want to export activity reports so that I can share insights with management.

### Teacher

- As a Teacher, I want to see parent context (child name, grade, section) when I receive a message so that I know which student it relates to.
- As a Teacher, I want real-time notifications when approved messages arrive so that I can respond promptly.
- As a Teacher, I want to send announcements to my class parents so that I can broadcast important updates.

### Parent

- As a Parent, I want to log in with email or phone OTP so that I can access the platform conveniently.
- As a Parent, I want to message the school about my child so that I can communicate without direct teacher contact.
- As a Parent, I want to see my children's information so that I know the system has the correct details.
- As a Parent, I want notifications when my message is approved or rejected so that I stay informed.
- As a Parent, I want to use the app in Arabic or English so that I can read content in my language.

---

## Functional Requirements

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-001 | Secure login via email/password | Must |
| FR-002 | Secure login via phone OTP | Must |
| FR-003 | JWT + refresh token authentication | Must |
| FR-004 | Role-based access control (5 roles) | Must |
| FR-005 | Parent message admin moderation workflow | Must |
| FR-006 | Real-time messaging via SignalR | Must |
| FR-007 | File/image/video attachments with limits | Must |
| FR-008 | Parent-child relationship management | Must |
| FR-009 | Auto-display student context in conversations | Must |
| FR-010 | Admin-only WhatsApp integration | Must |
| FR-011 | Bilingual UI (Arabic/English) with RTL | Must |
| FR-012 | Dark/light mode | Must |
| FR-013 | Push, email, SMS, in-app notifications | Should |
| FR-014 | MFA for admin accounts | Should |
| FR-015 | Message search | Should |
| FR-016 | PDF/Excel report export | Should |
| FR-017 | AI content moderation | Could |
| FR-018 | Multi-tenant SaaS with school branding | Must |

---

## Non-Functional Requirements

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-001 | Availability | 99.9% uptime |
| NFR-002 | API response time (p95) | < 200ms |
| NFR-003 | Concurrent users | 10,000+ |
| NFR-004 | Total users | 100,000+ |
| NFR-005 | WCAG accessibility | AA compliance |
| NFR-006 | Data encryption at rest | AES-256 |
| NFR-007 | Audit log retention | 7 years (configurable) |
| NFR-008 | Rate limiting | 100 req/min per user |
| NFR-009 | RTO / RPO | 4h / 1h |
| NFR-010 | Mobile parity | iOS + Android via Expo |

---

## Development Roadmap

### Phase 1 — Foundation (Weeks 1–4) ✅ Current
- Project scaffolding (backend, frontend, docs)
- Authentication (email + OTP)
- i18n (AR/EN, RTL)
- Login UI + role-based routing
- Admin WhatsApp module (API integration skeleton)
- Docker Compose (PostgreSQL, Redis)

### Phase 2 — Core Messaging (Weeks 5–8)
- Conversations, messages, attachments
- Moderation queue + admin dashboard
- SignalR real-time hub
- Student/parent relationship CRUD

### Phase 3 — Notifications & Mobile (Weeks 9–12)
- FCM push, email, SMS
- React Native Expo app
- Shared API client package

### Phase 4 — Enterprise (Weeks 13–16)
- Multi-tenant onboarding
- Reporting & analytics
- Elasticsearch message search
- AI moderation hooks

### Phase 5 — Production Hardening (Weeks 17–20)
- Kubernetes manifests
- OpenTelemetry + Grafana
- Load testing, security audit
- Production deployment

---

## Production Deployment Plan

1. **Infrastructure:** AWS/GCP/Azure with managed PostgreSQL, Redis, S3/R2
2. **CI/CD:** GitHub Actions → build → test → push images → deploy to K8s
3. **Secrets:** HashiCorp Vault or cloud secret manager
4. **DNS:** Cloudflare with WAF + DDoS protection
5. **Monitoring:** Prometheus + Grafana + alerting (PagerDuty)
6. **Backups:** Daily PostgreSQL snapshots, cross-region replication
7. **Blue/Green:** Zero-downtime deployments via K8s rolling updates
