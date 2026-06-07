# UI/UX Wireframes (Text Description)

## Login Page

```
┌─────────────────────────────────────────────────────────────┐
│  [Brand Panel - 50%]          │  [Login Panel - 50%]        │
│  Gradient blue background     │  [AR/EN] [Dark/Light]       │
│  SchoolCom logo + tagline     │                             │
│  Inspirational quote          │  Welcome back               │
│                               │  Sign in subtitle           │
│                               │  ┌─────────┬─────────┐      │
│                               │  │ Email   │ Phone   │      │
│                               │  └─────────┴─────────┘      │
│                               │  [Email/Password form]      │
│                               │  or [Phone → OTP flow]      │
│                               │  🔒 Secure connection       │
└─────────────────────────────────────────────────────────────┘
```

**Mobile:** Single column, brand header collapses to compact logo bar.

---

## Admin Dashboard

```
┌──────────┬──────────────────────────────────────────────────┐
│ Sidebar  │  Welcome, {name}              [Lang] [Theme]     │
│ ──────── │──────────────────────────────────────────────────│
│ Dashboard│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐   │
│ Moderate │  │Pending │ │Active  │ │Students│ │Notifs  │   │
│ WhatsApp │  │  12    │ │  48    │ │ 1,240  │ │   5    │   │
│ Students │  └────────┘ └────────┘ └────────┘ └────────┘   │
│ Reports  │  ┌─────────────────┐ ┌─────────────────┐         │
│ Settings │  │ Quick Actions   │ │ Recent Activity │         │
│          │  └─────────────────┘ └─────────────────┘         │
│ [Logout] │                                                  │
└──────────┴──────────────────────────────────────────────────┘
```

---

## Admin WhatsApp Page

```
┌─────────────────────────────────────────────────────────────┐
│  💬 WhatsApp Communication                                  │
│  Admin-only channel subtitle                                │
│  ┌ Status: Connected / dev-phone-id ─────────────────────┐ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌ Send Message ────────┐  ┌ Recent Conversations ──────┐ │
│  │ Phone: +966...       │  │ +966501234567              │ │
│  │ Message: [textarea]  │  │ Last message preview...    │ │
│  │ [Send via WhatsApp]  │  │ timestamp                  │ │
│  └──────────────────────┘  └────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## Messaging Thread (Phase 2)

Inspired by WhatsApp/Slack hybrid:

- Left: conversation list with unread badges
- Center: message thread with student context header
- Right (desktop): parent + child info panel

**Student Context Header:**
```
Parent: Ahmed Al-Rashid | Child: Sara (Grade 5, Section A) | Teacher: Ms. Johnson
```

---

## RTL (Arabic) Layout

- Sidebar moves to right
- Text alignment: right
- Icons and navigation order mirrored
- Phone numbers remain LTR (`dir="ltr"`)

---

## Color System

| Token | Light | Dark |
|-------|-------|------|
| Primary | #2563eb | #3b82f6 |
| Background | #f8fafc | #0b1120 |
| Card | #ffffff | #111827 |
| WhatsApp accent | #25D366 | #25D366 |

---

## Accessibility (WCAG AA)

- Minimum contrast ratio 4.5:1 for text
- Focus rings on all interactive elements
- Keyboard navigation for tabs and forms
- `aria-label` on icon-only buttons
- Screen reader labels for OTP inputs
