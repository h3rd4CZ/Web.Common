using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common;
using RhDev.Common.Web.Core.DataAccess.Sql.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Configuration
{
    public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
    {
        public void Configure(EntityTypeBuilder<AuditTrail> builder)
        {
            builder.Property(t => t.AuditType)
               .HasConversion<string>();
            builder.Property(e => e.AffectedColumns).HasStringListConversion();
            builder.Property(u => u.OldValues).HasJsonConversion();
            builder.Property(u => u.NewValues).HasJsonConversion();
            builder.Property(u => u.PrimaryKey).HasJsonConversion();
            builder.Ignore(x => x.TemporaryProperties);
            builder.Ignore(x => x.HasTemporaryProperties);
        }
    }
}
