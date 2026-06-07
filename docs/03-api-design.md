# API Design — SchoolCom REST API v1

Base URL: `https://api.schoolcom.com/v1`

All responses use envelope: `{ "data": T, "meta": {}, "errors": [] }`

---

## Authentication

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/auth/login` | Email + password login | Public |
| POST | `/auth/otp/request` | Request phone/email OTP | Public |
| POST | `/auth/otp/verify` | Verify OTP and issue tokens | Public |
| POST | `/auth/refresh` | Rotate refresh token | Cookie |
| POST | `/auth/logout` | Revoke refresh token | Bearer |
| POST | `/auth/register` | Parent self-registration (if enabled) | Public |
| POST | `/auth/forgot-password` | Send reset email | Public |
| POST | `/auth/reset-password` | Reset with token | Public |
| POST | `/auth/verify-email` | Email verification | Public |
| POST | `/auth/mfa/setup` | Enable MFA | Bearer |
| POST | `/auth/mfa/verify` | Verify MFA during login | Public |

### POST `/auth/login`

```json
// Request
{
  "email": "parent@example.com",
  "password": "SecurePass123!",
  "mfaCode": "123456"
}

// Response 200
{
  "data": {
    "accessToken": "eyJ...",
    "expiresIn": 900,
    "user": {
      "id": "uuid",
      "email": "parent@example.com",
      "roles": ["Parent"],
      "preferredLanguage": "ar",
      "tenantId": "uuid"
    }
  }
}
```

### POST `/auth/otp/request`

```json
{
  "identifier": "+966501234567",
  "channel": "sms"
}
```

---

## Users & Profiles

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/users/me` | All |
| PATCH | `/users/me` | All |
| GET | `/users/me/children` | Parent |
| GET | `/users/{id}` | Admin |

---

## Conversations & Messages

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/conversations` | List user conversations |
| POST | `/conversations` | Create (Parent → subject + student) |
| GET | `/conversations/{id}` | Get with participants + student context |
| GET | `/conversations/{id}/messages` | Paginated history |
| POST | `/conversations/{id}/messages` | Send message (→ moderation if parent) |
| POST | `/conversations/{id}/messages/{msgId}/read` | Mark read |
| GET | `/messages/search?q=` | Full-text search |

### Message Send (Parent)

```json
{
  "content": "Question about homework",
  "attachments": [
    { "uploadId": "uuid", "type": "image" }
  ],
  "studentId": "uuid"
}
```

Response: `moderationStatus: "Pending"`

---

## Moderation (School Admin)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/moderation/queue` | Pending items (filter by type) |
| GET | `/moderation/queue/stats` | Counts by type |
| POST | `/moderation/{messageId}/approve` | Approve |
| POST | `/moderation/{messageId}/reject` | Reject with reason |
| POST | `/moderation/{messageId}/request-changes` | Request changes |

---

## Media Upload

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/media/presign` | Get pre-signed upload URL |
| POST | `/media/confirm` | Confirm upload + trigger scan |

---

## WhatsApp Admin (Admin Only)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/admin/whatsapp/config` | Get integration status |
| POST | `/admin/whatsapp/config` | Configure Business API |
| POST | `/admin/whatsapp/send` | Send message to parent phone |
| GET | `/admin/whatsapp/conversations` | List WhatsApp threads |
| GET | `/admin/whatsapp/webhook` | Meta webhook verification |
| POST | `/admin/whatsapp/webhook` | Inbound messages |

---

## Notifications

| Method | Endpoint |
|--------|----------|
| GET | `/notifications` |
| PATCH | `/notifications/{id}/read` |
| PATCH | `/notifications/read-all` |
| GET | `/notifications/preferences` |
| PUT | `/notifications/preferences` |

---

## Reports & Analytics

| Method | Endpoint | Roles |
|--------|----------|-------|
| GET | `/reports/parent-activity` | Admin |
| GET | `/reports/teacher-activity` | Admin |
| GET | `/reports/message-stats` | Admin |
| GET | `/reports/moderation` | Admin |
| GET | `/reports/export?format=pdf\|xlsx` | Admin |
| GET | `/analytics/engagement` | Admin, SuperAdmin |

---

## SignalR Hubs

| Hub | Events |
|-----|--------|
| `/hubs/messaging` | `ReceiveMessage`, `MessageApproved`, `Typing`, `ReadReceipt`, `ModerationUpdated` |
| `/hubs/notifications` | `NewNotification` |

---

## Error Codes

| Code | HTTP | Description |
|------|------|-------------|
| `AUTH_INVALID_CREDENTIALS` | 401 | Wrong email/password |
| `AUTH_OTP_EXPIRED` | 401 | OTP expired |
| `AUTH_MFA_REQUIRED` | 403 | MFA step needed |
| `MODERATION_PENDING` | 403 | Content not yet approved |
| `MEDIA_SIZE_EXCEEDED` | 413 | File too large |
| `MEDIA_TYPE_INVALID` | 415 | Unsupported format |
| `TENANT_FORBIDDEN` | 403 | Cross-tenant access |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |

---

## API Versioning

Header: `X-Api-Version: 1.0` or URL prefix `/v1/`

OpenAPI spec: `/swagger/v1/swagger.json`
