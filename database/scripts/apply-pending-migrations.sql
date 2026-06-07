-- Pending migrations not yet applied to schoolcom on port 5400.
-- 20260603140000_AddJunctionTableAuditTimestamps

ALTER TABLE "UserRoleAssignments"
  ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT '2026-01-01 00:00:00+00';

ALTER TABLE "UserRoleAssignments"
  ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamp with time zone NULL;

ALTER TABLE "ParentStudents"
  ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT '2026-01-01 00:00:00+00';

ALTER TABLE "ParentStudents"
  ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamp with time zone NULL;

ALTER TABLE "ConversationParticipants"
  ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT '2026-01-01 00:00:00+00';

ALTER TABLE "ConversationParticipants"
  ADD COLUMN IF NOT EXISTS "UpdatedAt" timestamp with time zone NULL;

UPDATE "ConversationParticipants"
SET "CreatedAt" = "JoinedAt"
WHERE "CreatedAt" = '2026-01-01 00:00:00+00';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260603140000_AddJunctionTableAuditTimestamps', '10.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
