using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolCom.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class EnforceClassSectionUniqueConstraint : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_sections_TenantId_SchoolClassId_NameEn",
            table: "sections");

        migrationBuilder.CreateIndex(
            name: "UQ_class_sections_school_class_id_section_name",
            table: "sections",
            columns: new[] { "TenantId", "SchoolClassId", "NameEn" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UQ_class_sections_school_class_id_section_name",
            table: "sections");

        migrationBuilder.CreateIndex(
            name: "IX_sections_TenantId_SchoolClassId_NameEn",
            table: "sections",
            columns: new[] { "TenantId", "SchoolClassId", "NameEn" },
            unique: true);
    }
}
