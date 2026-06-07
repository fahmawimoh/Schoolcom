using SchoolCom.Domain.Common;

namespace SchoolCom.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string? BrandingJson { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
    public TenantSettings? Settings { get; set; }
}

public class TenantSettings : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public bool RequireTeacherMessageModeration { get; set; }
    public bool EnableWhatsAppIntegration { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public int MaxImageSizeMb { get; set; } = 2;
    public int MaxVideoSizeMb { get; set; } = 20;
    public int MaxDocumentSizeMb { get; set; } = 3;
}
