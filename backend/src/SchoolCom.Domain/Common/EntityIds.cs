namespace SchoolCom.Domain.Common;

/// <summary>
/// Time-ordered UUID v7 identifiers for primary keys (PostgreSQL <c>uuid</c>).
/// Prefer over v4 (<see cref="Guid.NewGuid"/>) for index locality and debuggability.
/// </summary>
public static class EntityIds
{
    public static Guid New() => Guid.CreateVersion7();
}
