using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SchoolCom.Api.Middleware;

public class DatabaseExceptionMiddleware(RequestDelegate next, ILogger<DatabaseExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NpgsqlException ex) when (ex is not PostgresException)
        {
            logger.LogError(ex, "PostgreSQL connection error");
            await WriteProblemAsync(context, (
                HttpStatusCode.ServiceUnavailable,
                "Database unavailable",
                "Unable to reach PostgreSQL. Verify the server is running and connection settings in backend/.env."));
        }
        catch (PostgresException ex)
        {
            logger.LogWarning(ex, "PostgreSQL error {SqlState}", ex.SqlState);
            await WriteProblemAsync(context, MapPostgresError(ex));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            logger.LogWarning(pg, "EF database update error {SqlState}", pg.SqlState);
            await WriteProblemAsync(context, MapPostgresError(pg));
        }
    }

    private static bool IsClassSectionConstraint(string? constraintName) =>
        constraintName is "UQ_class_sections_school_class_id_section_id"
            or "IX_sections_TenantId_SchoolClassId_NameEn"
            or "UQ_class_sections_school_class_id_section_name"
            or "UQ_sections_tenant_name_en"
            or "UQ_school_classes_tenant_name_en"
            or "UQ_rooms_tenant_code"
            or "UQ_room_assignments_class_section_id"
            or "UQ_room_assignments_room_id";

    private static (HttpStatusCode Status, string Title, string Detail) MapPostgresError(PostgresException ex) =>
        ex.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation when IsClassSectionConstraint(ex.ConstraintName) =>
                (HttpStatusCode.Conflict, "Duplicate entry", "This class-section combination already exists."),
            PostgresErrorCodes.UniqueViolation =>
                (HttpStatusCode.Conflict, "Duplicate entry", "A record with the same unique value already exists."),
            PostgresErrorCodes.ForeignKeyViolation => (HttpStatusCode.BadRequest, "Invalid reference", "The operation references a record that does not exist."),
            PostgresErrorCodes.NotNullViolation => (HttpStatusCode.BadRequest, "Missing required field", "A required value was not provided."),
            PostgresErrorCodes.CheckViolation => (HttpStatusCode.BadRequest, "Validation failed", "The value does not meet database constraints."),
            _ => (HttpStatusCode.InternalServerError, "Database error", "An unexpected database error occurred.")
        };

    private static async Task WriteProblemAsync(HttpContext context, (HttpStatusCode Status, string Title, string Detail) problem)
    {
        if (context.Response.HasStarted) throw new InvalidOperationException("Response already started.");

        context.Response.StatusCode = (int)problem.Status;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new
        {
            title = problem.Title,
            detail = problem.Detail,
            status = (int)problem.Status
        });

        await context.Response.WriteAsync(body);
    }
}
