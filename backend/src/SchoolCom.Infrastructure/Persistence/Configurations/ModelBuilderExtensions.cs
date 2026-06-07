using Microsoft.EntityFrameworkCore;
using SchoolCom.Domain.Common;
using SchoolCom.Infrastructure.Persistence.ValueGenerators;

namespace SchoolCom.Infrastructure.Persistence.Configurations;

public static class ModelBuilderExtensions
{
    public static void ApplyUuidV7PrimaryKeys(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;

            modelBuilder.Entity(entityType.ClrType)
                .Property<Guid>(nameof(BaseEntity.Id))
                .ValueGeneratedOnAdd()
                .HasValueGenerator<UuidV7ValueGenerator>();
        }
    }
}
