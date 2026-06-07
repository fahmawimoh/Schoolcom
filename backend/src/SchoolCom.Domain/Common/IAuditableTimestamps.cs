namespace SchoolCom.Domain.Common;

public interface IAuditableTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
