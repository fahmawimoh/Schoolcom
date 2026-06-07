using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using SchoolCom.Domain.Common;

namespace SchoolCom.Infrastructure.Persistence.ValueGenerators;

public sealed class UuidV7ValueGenerator : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry) => EntityIds.New();
}
