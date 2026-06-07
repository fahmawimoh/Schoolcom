using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCom.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create school_classes from Grades
            migrationBuilder.CreateTable(
                name: "school_classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DescriptionAr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_school_classes", x => x.Id));

            migrationBuilder.Sql("""
                INSERT INTO school_classes ("Id", "NameEn", "NameAr", "SortOrder", "IsActive", "CreatedAt", "UpdatedAt", "TenantId")
                SELECT "Id", "Name", "Name", "Level", true, "CreatedAt", "UpdatedAt", "TenantId"
                FROM "Grades";
                """);

            // 2. Add bilingual columns to Sections (keep table name Sections initially)
            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Sections",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Sections",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Sections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Sections",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sections",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                UPDATE "Sections"
                SET "NameEn" = "Name",
                    "NameAr" = CASE
                        WHEN "Name" = 'A' THEN 'الشعبة أ'
                        WHEN "Name" = 'B' THEN 'الشعبة ب'
                        WHEN "Name" = 'C' THEN 'الشعبة ج'
                        ELSE "Name"
                    END;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Sections_TenantId_Name",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Sections");

            // 3. Create class_sections from Classrooms
            migrationBuilder.CreateTable(
                name: "class_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    HomeroomTeacherUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_class_sections_SchoolBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "SchoolBranches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_class_sections_Users_HomeroomTeacherUserId",
                        column: x => x.HomeroomTeacherUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_class_sections_school_classes_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "school_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_sections_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO class_sections ("Id", "SchoolClassId", "SectionId", "BranchId", "HomeroomTeacherUserId", "CreatedAt", "UpdatedAt", "TenantId")
                SELECT "Id", "GradeId", "SectionId", "BranchId", "HomeroomTeacherId", "CreatedAt", "UpdatedAt", "TenantId"
                FROM "Classrooms";
                """);

            // 4. Create teachers table
            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstNameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastNameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubjectEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubjectAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teachers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "teacher_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_teacher_assignments_class_sections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalTable: "class_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_assignments_teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 5. Update Students - add new columns and migrate data
            migrationBuilder.AddColumn<string>(
                name: "FirstNameEn",
                table: "Students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstNameAr",
                table: "Students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastNameEn",
                table: "Students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastNameAr",
                table: "Students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolClassId",
                table: "Students",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "Students",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Students",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Students" s
                SET "FirstNameEn" = s."FirstName",
                    "FirstNameAr" = s."FirstName",
                    "LastNameEn" = s."LastName",
                    "LastNameAr" = s."LastName",
                    "SchoolClassId" = c."GradeId",
                    "SectionId" = c."SectionId"
                FROM "Classrooms" c
                WHERE s."ClassroomId" = c."Id";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Classrooms_ClassroomId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_ClassroomId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_TenantId_ClassroomId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ClassroomId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Students");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolClassId",
                table: "Students",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SectionId",
                table: "Students",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // 6. Drop old tables
            migrationBuilder.DropTable(name: "Classrooms");
            migrationBuilder.DropTable(name: "Grades");

            // 7. Rename Sections to sections
            migrationBuilder.RenameTable(
                name: "Sections",
                newName: "sections");

            // 8. Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentId",
                table: "Students",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolClassId",
                table: "Students",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SectionId",
                table: "Students",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_TenantId_SchoolClassId",
                table: "Students",
                columns: new[] { "TenantId", "SchoolClassId" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_TenantId_SectionId",
                table: "Students",
                columns: new[] { "TenantId", "SectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_sections_TenantId_IsActive",
                table: "sections",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_sections_TenantId_NameEn",
                table: "sections",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_sections_BranchId",
                table: "class_sections",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_class_sections_HomeroomTeacherUserId",
                table: "class_sections",
                column: "HomeroomTeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_class_sections_SchoolClassId",
                table: "class_sections",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_class_sections_SectionId",
                table: "class_sections",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_class_sections_TenantId_SchoolClassId_SectionId",
                table: "class_sections",
                columns: new[] { "TenantId", "SchoolClassId", "SectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_school_classes_TenantId_IsActive",
                table: "school_classes",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_school_classes_TenantId_SortOrder",
                table: "school_classes",
                columns: new[] { "TenantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_ClassSectionId",
                table: "teacher_assignments",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_TeacherId_ClassSectionId",
                table: "teacher_assignments",
                columns: new[] { "TeacherId", "ClassSectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teachers_TenantId_Email",
                table: "teachers",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_teachers_TenantId_EmployeeNumber",
                table: "teachers",
                columns: new[] { "TenantId", "EmployeeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teachers_TenantId_IsActive",
                table: "teachers",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_teachers_UserId",
                table: "teachers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_ParentId",
                table: "Students",
                column: "ParentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_school_classes_SchoolClassId",
                table: "Students",
                column: "SchoolClassId",
                principalTable: "school_classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_sections_SectionId",
                table: "Students",
                column: "SectionId",
                principalTable: "sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Down migration is not supported for AddAcademicManagementModule.");
        }
    }
}
