using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCom.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class RestoreStandaloneSectionsAndJunction : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Create class_sections junction from existing per-class sections
        migrationBuilder.CreateTable(
            name: "class_sections",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SchoolClassId = table.Column<Guid>(type: "uuid", nullable: false),
                SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_class_sections", x => x.Id);
                table.ForeignKey(
                    name: "FK_class_sections_school_classes_SchoolClassId",
                    column: x => x.SchoolClassId,
                    principalTable: "school_classes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_class_sections_sections_SectionId",
                    column: x => x.SectionId,
                    principalTable: "sections",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.Sql("""
            INSERT INTO class_sections ("Id", "SchoolClassId", "SectionId", "IsActive", "CreatedAt", "TenantId")
            SELECT gen_random_uuid(), "SchoolClassId", "Id", "IsActive", "CreatedAt", "TenantId"
            FROM sections
            WHERE "SchoolClassId" IS NOT NULL;
            """);

        // 2. Deduplicate sections by tenant + name, keeping earliest row per name
        migrationBuilder.Sql("""
            CREATE TEMP TABLE section_dedup AS
            SELECT s."Id" AS old_id,
                   FIRST_VALUE(s."Id") OVER (
                       PARTITION BY s."TenantId", LOWER(TRIM(s."NameEn"))
                       ORDER BY s."CreatedAt", s."Id"
                   ) AS canonical_id
            FROM sections s;

            UPDATE class_sections cs
            SET "SectionId" = d.canonical_id
            FROM section_dedup d
            WHERE cs."SectionId" = d.old_id AND d.old_id <> d.canonical_id;

            UPDATE "Students" st
            SET "SectionId" = d.canonical_id
            FROM section_dedup d
            WHERE st."SectionId" = d.old_id AND d.old_id <> d.canonical_id;

            UPDATE teacher_assignments ta
            SET "SectionId" = d.canonical_id
            FROM section_dedup d
            WHERE ta."SectionId" = d.old_id AND d.old_id <> d.canonical_id;

            UPDATE timetable_entries te
            SET "SectionId" = d.canonical_id
            FROM section_dedup d
            WHERE te."SectionId" = d.old_id AND d.old_id <> d.canonical_id;

            DELETE FROM sections s
            USING section_dedup d
            WHERE s."Id" = d.old_id AND d.old_id <> d.canonical_id;

            DROP TABLE section_dedup;
            """);

        // 3. Add ClassSectionId to dependent tables
        migrationBuilder.AddColumn<Guid>(
            name: "ClassSectionId",
            table: "Students",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ClassSectionId",
            table: "teacher_assignments",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ClassSectionId",
            table: "timetable_entries",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE "Students" st
            SET "ClassSectionId" = cs."Id"
            FROM class_sections cs
            WHERE cs."SchoolClassId" = st."SchoolClassId"
              AND cs."SectionId" = st."SectionId"
              AND cs."TenantId" = st."TenantId";

            UPDATE teacher_assignments ta
            SET "ClassSectionId" = cs."Id"
            FROM class_sections cs
            WHERE ta."SectionId" IS NOT NULL
              AND cs."SchoolClassId" = ta."SchoolClassId"
              AND cs."SectionId" = ta."SectionId";

            UPDATE timetable_entries te
            SET "ClassSectionId" = cs."Id"
            FROM class_sections cs
            WHERE cs."SchoolClassId" = te."SchoolClassId"
              AND cs."SectionId" = te."SectionId"
              AND cs."TenantId" = te."TenantId";
            """);

        // 4. Drop old section-to-class coupling
        migrationBuilder.DropForeignKey(
            name: "FK_sections_school_classes_SchoolClassId",
            table: "sections");

        migrationBuilder.DropForeignKey(
            name: "FK_sections_Users_HomeroomTeacherUserId",
            table: "sections");

        migrationBuilder.DropIndex(
            name: "UQ_class_sections_school_class_id_section_name",
            table: "sections");

        migrationBuilder.DropIndex(
            name: "IX_sections_SchoolClassId",
            table: "sections");

        migrationBuilder.DropIndex(
            name: "IX_sections_HomeroomTeacherUserId",
            table: "sections");

        migrationBuilder.DropColumn(
            name: "SchoolClassId",
            table: "sections");

        migrationBuilder.DropColumn(
            name: "HomeroomTeacherUserId",
            table: "sections");

        // 5. New unique indexes
        migrationBuilder.CreateIndex(
            name: "UQ_sections_tenant_name_en",
            table: "sections",
            columns: new[] { "TenantId", "NameEn" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_class_sections_school_class_id_section_id",
            table: "class_sections",
            columns: new[] { "TenantId", "SchoolClassId", "SectionId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_class_sections_SchoolClassId",
            table: "class_sections",
            column: "SchoolClassId");

        migrationBuilder.CreateIndex(
            name: "IX_class_sections_SectionId",
            table: "class_sections",
            column: "SectionId");

        migrationBuilder.CreateIndex(
            name: "UQ_school_classes_tenant_name_en",
            table: "school_classes",
            columns: new[] { "TenantId", "NameEn" },
            unique: true);

        migrationBuilder.DropIndex(
            name: "IX_rooms_TenantId_Code",
            table: "rooms");

        migrationBuilder.CreateIndex(
            name: "UQ_rooms_tenant_code",
            table: "rooms",
            columns: new[] { "TenantId", "Code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Students_ClassSectionId",
            table: "Students",
            column: "ClassSectionId");

        migrationBuilder.CreateIndex(
            name: "IX_teacher_assignments_ClassSectionId",
            table: "teacher_assignments",
            column: "ClassSectionId");

        migrationBuilder.CreateIndex(
            name: "IX_timetable_entries_ClassSectionId",
            table: "timetable_entries",
            column: "ClassSectionId");

        migrationBuilder.AddForeignKey(
            name: "FK_Students_class_sections_ClassSectionId",
            table: "Students",
            column: "ClassSectionId",
            principalTable: "class_sections",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_teacher_assignments_class_sections_ClassSectionId",
            table: "teacher_assignments",
            column: "ClassSectionId",
            principalTable: "class_sections",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_timetable_entries_class_sections_ClassSectionId",
            table: "timetable_entries",
            column: "ClassSectionId",
            principalTable: "class_sections",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        // 6. Room assignments table
        migrationBuilder.CreateTable(
            name: "room_assignments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ClassSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_room_assignments", x => x.Id);
                table.ForeignKey(
                    name: "FK_room_assignments_class_sections_ClassSectionId",
                    column: x => x.ClassSectionId,
                    principalTable: "class_sections",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_room_assignments_rooms_RoomId",
                    column: x => x.RoomId,
                    principalTable: "rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "UQ_room_assignments_class_section_id",
            table: "room_assignments",
            column: "ClassSectionId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_room_assignments_room_id",
            table: "room_assignments",
            column: "RoomId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "room_assignments");

        migrationBuilder.DropForeignKey(
            name: "FK_Students_class_sections_ClassSectionId",
            table: "Students");

        migrationBuilder.DropForeignKey(
            name: "FK_teacher_assignments_class_sections_ClassSectionId",
            table: "teacher_assignments");

        migrationBuilder.DropForeignKey(
            name: "FK_timetable_entries_class_sections_ClassSectionId",
            table: "timetable_entries");

        migrationBuilder.DropTable(name: "class_sections");

        migrationBuilder.DropIndex(name: "UQ_sections_tenant_name_en", table: "sections");
        migrationBuilder.DropIndex(name: "UQ_school_classes_tenant_name_en", table: "school_classes");
        migrationBuilder.DropIndex(name: "UQ_rooms_tenant_code", table: "rooms");

        migrationBuilder.DropColumn(name: "ClassSectionId", table: "Students");
        migrationBuilder.DropColumn(name: "ClassSectionId", table: "teacher_assignments");
        migrationBuilder.DropColumn(name: "ClassSectionId", table: "timetable_entries");

        migrationBuilder.AddColumn<Guid>(
            name: "SchoolClassId",
            table: "sections",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddColumn<Guid>(
            name: "HomeroomTeacherUserId",
            table: "sections",
            type: "uuid",
            nullable: true);
    }
}
