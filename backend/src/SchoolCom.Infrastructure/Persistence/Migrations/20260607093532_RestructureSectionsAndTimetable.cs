using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCom.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestructureSectionsAndTimetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create subjects and rooms
            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_subjects", x => x.Id));

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_rooms", x => x.Id));

            migrationBuilder.Sql("""
                INSERT INTO subjects ("Id", "NameEn", "NameAr", "Code", "IsActive", "CreatedAt", "TenantId")
                SELECT gen_random_uuid(), 'Mathematics', 'الرياضيات', 'MATH', true, NOW(), t."Id"
                FROM "Tenants" t
                WHERE NOT EXISTS (SELECT 1 FROM subjects s WHERE s."TenantId" = t."Id" AND s."Code" = 'MATH');
                """);

            // 2. Add new columns to sections (nullable first)
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolClassId",
                table: "sections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HomeroomTeacherUserId",
                table: "sections",
                type: "uuid",
                nullable: true);

            // Drop tenant-wide name uniqueness before per-class section copies
            migrationBuilder.DropIndex(
                name: "IX_sections_TenantId_NameEn",
                table: "sections");

            // 3. Create per-class section copies, update students, delete old sections
            migrationBuilder.Sql("""
                CREATE TEMP TABLE _section_map (
                    old_section_id uuid NOT NULL,
                    school_class_id uuid NOT NULL,
                    new_section_id uuid NOT NULL,
                    PRIMARY KEY (old_section_id, school_class_id)
                );

                INSERT INTO _section_map (old_section_id, school_class_id, new_section_id)
                SELECT cs."SectionId", cs."SchoolClassId", gen_random_uuid()
                FROM class_sections cs;

                INSERT INTO sections (
                    "Id", "TenantId", "SchoolClassId", "NameEn", "NameAr",
                    "DescriptionEn", "DescriptionAr", "IsActive",
                    "HomeroomTeacherUserId", "CreatedAt", "UpdatedAt")
                SELECT
                    sm.new_section_id,
                    s."TenantId",
                    sm.school_class_id,
                    s."NameEn",
                    s."NameAr",
                    s."DescriptionEn",
                    s."DescriptionAr",
                    s."IsActive",
                    cs."HomeroomTeacherUserId",
                    NOW(),
                    NOW()
                FROM _section_map sm
                JOIN sections s ON s."Id" = sm.old_section_id
                JOIN class_sections cs ON cs."SchoolClassId" = sm.school_class_id AND cs."SectionId" = sm.old_section_id;

                UPDATE "Students" st
                SET "SectionId" = sm.new_section_id
                FROM _section_map sm
                WHERE st."SchoolClassId" = sm.school_class_id
                  AND st."SectionId" = sm.old_section_id;

                DELETE FROM teacher_assignments;
                DELETE FROM class_sections;

                DELETE FROM sections s
                WHERE s."SchoolClassId" IS NULL
                  AND EXISTS (SELECT 1 FROM _section_map sm WHERE sm.old_section_id = s."Id");

                DROP TABLE _section_map;
                """);

            // 4. Rebuild teacher_assignments with new shape (data already cleared above)
            migrationBuilder.DropForeignKey(
                name: "FK_teacher_assignments_class_sections_ClassSectionId",
                table: "teacher_assignments");

            migrationBuilder.DropIndex(
                name: "IX_teacher_assignments_TeacherId_ClassSectionId",
                table: "teacher_assignments");

            migrationBuilder.DropIndex(
                name: "IX_teacher_assignments_ClassSectionId",
                table: "teacher_assignments");

            migrationBuilder.DropColumn(
                name: "ClassSectionId",
                table: "teacher_assignments");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolClassId",
                table: "teacher_assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "teacher_assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "teacher_assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // 5. Drop class_sections and old shared sections
            migrationBuilder.DropTable(name: "class_sections");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolClassId",
                table: "sections",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // 6. Create timetable_entries
            migrationBuilder.CreateTable(
                name: "timetable_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timetable_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_timetable_entries_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_school_classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "school_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 7. Indexes and FKs
            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_SchoolClassId",
                table: "teacher_assignments",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_SectionId",
                table: "teacher_assignments",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_SubjectId",
                table: "teacher_assignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_TeacherId_SchoolClassId_SectionId_SubjectId",
                table: "teacher_assignments",
                columns: new[] { "TeacherId", "SchoolClassId", "SectionId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sections_HomeroomTeacherUserId",
                table: "sections",
                column: "HomeroomTeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_sections_SchoolClassId",
                table: "sections",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_sections_TenantId_SchoolClassId_NameEn",
                table: "sections",
                columns: new[] { "TenantId", "SchoolClassId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_TenantId_Code",
                table: "rooms",
                columns: new[] { "TenantId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_rooms_TenantId_IsActive",
                table: "rooms",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_subjects_TenantId_Code",
                table: "subjects",
                columns: new[] { "TenantId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_subjects_TenantId_IsActive",
                table: "subjects",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_RoomId",
                table: "timetable_entries",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_SchoolClassId",
                table: "timetable_entries",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_SectionId",
                table: "timetable_entries",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_SubjectId",
                table: "timetable_entries",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_TeacherId",
                table: "timetable_entries",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_TenantId_SectionId_DayOfWeek_PeriodNumber",
                table: "timetable_entries",
                columns: new[] { "TenantId", "SectionId", "DayOfWeek", "PeriodNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_sections_Users_HomeroomTeacherUserId",
                table: "sections",
                column: "HomeroomTeacherUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_sections_school_classes_SchoolClassId",
                table: "sections",
                column: "SchoolClassId",
                principalTable: "school_classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_assignments_school_classes_SchoolClassId",
                table: "teacher_assignments",
                column: "SchoolClassId",
                principalTable: "school_classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_assignments_sections_SectionId",
                table: "teacher_assignments",
                column: "SectionId",
                principalTable: "sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_assignments_subjects_SubjectId",
                table: "teacher_assignments",
                column: "SubjectId",
                principalTable: "subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Down migration is not supported for RestructureSectionsAndTimetable.");
        }
    }
}
