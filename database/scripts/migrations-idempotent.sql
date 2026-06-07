CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Grades" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Level" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Grades" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "OtpCodes" (
        "Id" uuid NOT NULL,
        "Identifier" character varying(256) NOT NULL,
        "CodeHash" character varying(512) NOT NULL,
        "Channel" character varying(16) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "AttemptCount" integer NOT NULL,
        "IsUsed" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_OtpCodes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" character varying(32) NOT NULL,
        "DisplayName" character varying(100) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "SchoolBranches" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Address" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_SchoolBranches" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Sections" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Sections" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Tenants" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Slug" character varying(120) NOT NULL,
        "CustomDomain" character varying(255),
        "BrandingJson" jsonb,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Tenants" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "TenantSettings" (
        "Id" uuid NOT NULL,
        "TenantId" uuid NOT NULL,
        "RequireTeacherMessageModeration" boolean NOT NULL,
        "EnableWhatsAppIntegration" boolean NOT NULL,
        "PreferredLanguage" character varying(5) NOT NULL,
        "MaxImageSizeMb" integer NOT NULL,
        "MaxVideoSizeMb" integer NOT NULL,
        "MaxDocumentSizeMb" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_TenantSettings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TenantSettings_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Email" character varying(256) NOT NULL,
        "Phone" character varying(32),
        "PasswordHash" character varying(512) NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "EmailVerified" boolean NOT NULL,
        "PhoneVerified" boolean NOT NULL,
        "MfaEnabled" boolean NOT NULL,
        "MfaSecret" character varying(256),
        "PreferredLanguage" character varying(5) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Users_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "Action" character varying(100) NOT NULL,
        "EntityType" character varying(100) NOT NULL,
        "EntityId" uuid,
        "MetadataJson" jsonb,
        "IpAddress" character varying(45),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AuditLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Classrooms" (
        "Id" uuid NOT NULL,
        "GradeId" uuid NOT NULL,
        "SectionId" uuid NOT NULL,
        "HomeroomTeacherId" uuid,
        "BranchId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Classrooms" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Classrooms_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Classrooms_SchoolBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "SchoolBranches" ("Id"),
        CONSTRAINT "FK_Classrooms_Sections_SectionId" FOREIGN KEY ("SectionId") REFERENCES "Sections" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Classrooms_Users_HomeroomTeacherId" FOREIGN KEY ("HomeroomTeacherId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" character varying(512) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "ReplacedByTokenHash" character varying(512),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "UserRoleAssignments" (
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        CONSTRAINT "PK_UserRoleAssignments" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_UserRoleAssignments_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoleAssignments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "WhatsAppSessions" (
        "Id" uuid NOT NULL,
        "AdminUserId" uuid NOT NULL,
        "PhoneNumberId" character varying(64) NOT NULL,
        "AccessTokenEncrypted" character varying(2048) NOT NULL,
        "ConfigJson" jsonb,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_WhatsAppSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WhatsAppSessions_Users_AdminUserId" FOREIGN KEY ("AdminUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Students" (
        "Id" uuid NOT NULL,
        "StudentNumber" character varying(50) NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "ClassroomId" uuid NOT NULL,
        "BranchId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Students" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Students_Classrooms_ClassroomId" FOREIGN KEY ("ClassroomId") REFERENCES "Classrooms" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Students_SchoolBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "SchoolBranches" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "WhatsAppMessages" (
        "Id" uuid NOT NULL,
        "SessionId" uuid NOT NULL,
        "RecipientPhone" character varying(32) NOT NULL,
        "Body" character varying(4000) NOT NULL,
        "Direction" character varying(16) NOT NULL,
        "ExternalMessageId" character varying(128),
        "Status" character varying(32) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_WhatsAppMessages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WhatsAppMessages_WhatsAppSessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "WhatsAppSessions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Conversations" (
        "Id" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "Subject" character varying(300) NOT NULL,
        "Status" character varying(32) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Conversations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Conversations_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "ParentStudents" (
        "ParentId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "Relationship" character varying(50) NOT NULL,
        CONSTRAINT "PK_ParentStudents" PRIMARY KEY ("ParentId", "StudentId"),
        CONSTRAINT "FK_ParentStudents_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Students" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ParentStudents_Users_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "ConversationParticipants" (
        "ConversationId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "JoinedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ConversationParticipants" PRIMARY KEY ("ConversationId", "UserId"),
        CONSTRAINT "FK_ConversationParticipants_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ConversationParticipants_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "Messages" (
        "Id" uuid NOT NULL,
        "ConversationId" uuid NOT NULL,
        "SenderId" uuid NOT NULL,
        "Content" character varying(4000) NOT NULL,
        "MessageType" character varying(32) NOT NULL,
        "ModerationStatus" character varying(32) NOT NULL,
        "SentAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Messages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Messages_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Messages_Users_SenderId" FOREIGN KEY ("SenderId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "MessageAttachments" (
        "Id" uuid NOT NULL,
        "MessageId" uuid NOT NULL,
        "FileName" character varying(255) NOT NULL,
        "StorageKey" character varying(1024) NOT NULL,
        "MimeType" character varying(100) NOT NULL,
        "SizeBytes" bigint NOT NULL,
        "ModerationStatus" character varying(32) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_MessageAttachments" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_MessageAttachment_SizeBytes_Positive" CHECK ("SizeBytes" > 0),
        CONSTRAINT "FK_MessageAttachments_Messages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "Messages" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE TABLE "MessageModerations" (
        "Id" uuid NOT NULL,
        "MessageId" uuid NOT NULL,
        "ReviewerId" uuid NOT NULL,
        "Action" character varying(32) NOT NULL,
        "Notes" character varying(1000),
        "ReviewedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_MessageModerations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MessageModerations_Messages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "Messages" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_MessageModerations_Users_ReviewerId" FOREIGN KEY ("ReviewerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_AuditLogs_TenantId_CreatedAt" ON "AuditLogs" ("TenantId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Classrooms_BranchId" ON "Classrooms" ("BranchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Classrooms_GradeId" ON "Classrooms" ("GradeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Classrooms_HomeroomTeacherId" ON "Classrooms" ("HomeroomTeacherId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Classrooms_SectionId" ON "Classrooms" ("SectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Classrooms_TenantId_GradeId_SectionId" ON "Classrooms" ("TenantId", "GradeId", "SectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_ConversationParticipants_UserId" ON "ConversationParticipants" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Conversations_StudentId" ON "Conversations" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Conversations_TenantId_StudentId" ON "Conversations" ("TenantId", "StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Grades_TenantId_Level" ON "Grades" ("TenantId", "Level");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_MessageAttachments_MessageId" ON "MessageAttachments" ("MessageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_MessageModerations_MessageId_ReviewedAt" ON "MessageModerations" ("MessageId", "ReviewedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_MessageModerations_ReviewerId" ON "MessageModerations" ("ReviewerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Messages_ConversationId_SentAt" ON "Messages" ("ConversationId", "SentAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Messages_SenderId" ON "Messages" ("SenderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Messages_TenantId_ModerationStatus" ON "Messages" ("TenantId", "ModerationStatus");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_OtpCodes_Identifier_Channel_ExpiresAt" ON "OtpCodes" ("Identifier", "Channel", "ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_ParentStudents_StudentId" ON "ParentStudents" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_RefreshTokens_ExpiresAt" ON "RefreshTokens" ("ExpiresAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_UserId_TokenHash" ON "RefreshTokens" ("UserId", "TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_SchoolBranches_TenantId_Name" ON "SchoolBranches" ("TenantId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Sections_TenantId_Name" ON "Sections" ("TenantId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Students_BranchId" ON "Students" ("BranchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Students_ClassroomId" ON "Students" ("ClassroomId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Students_TenantId_ClassroomId" ON "Students" ("TenantId", "ClassroomId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Students_TenantId_StudentNumber" ON "Students" ("TenantId", "StudentNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Tenants_Slug" ON "Tenants" ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_TenantSettings_TenantId" ON "TenantSettings" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_UserRoleAssignments_RoleId" ON "UserRoleAssignments" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Users_Phone" ON "Users" ("Phone");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_Users_TenantId" ON "Users" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Users_TenantId_Email" ON "Users" ("TenantId", "Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_Users_TenantId_Phone" ON "Users" ("TenantId", "Phone");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_WhatsAppMessages_SessionId" ON "WhatsAppMessages" ("SessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_WhatsAppMessages_TenantId_SessionId_CreatedAt" ON "WhatsAppMessages" ("TenantId", "SessionId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE INDEX "IX_WhatsAppSessions_AdminUserId" ON "WhatsAppSessions" ("AdminUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    CREATE UNIQUE INDEX "IX_WhatsAppSessions_TenantId_AdminUserId" ON "WhatsAppSessions" ("TenantId", "AdminUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260602130522_InitialPostgresSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260602130522_InitialPostgresSchema', '10.0.0-preview.3.25171.6');
    END IF;
END $EF$;
COMMIT;

