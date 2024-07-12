using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhDev.Common.Web.Core.DataAccess.Sql.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Configuration
{
    public class WorkflowDocumentConfiguration : IEntityTypeConfiguration<WorkflowDocument>
    {
        public void Configure(EntityTypeBuilder<WorkflowDocument> builder)
        {
            builder.OwnsOne(s => s.WorkflowDocumentIdentificator,
                navigationBuilder =>
                {
                    navigationBuilder.ToJson();
                });
        }
    }

    public class WorkflowTransitionRequestConfiguration : IEntityTypeConfiguration<WorkflowTransitionRequest>
    {
        public void Configure(EntityTypeBuilder<WorkflowTransitionRequest> builder)
        {
            builder.Property(p => p.Payload).HasJsonConversion();
        }
    }

    public class WorkflowTransitionLogConfiguration : IEntityTypeConfiguration<WorkflowTransitionLog>
    {
        public void Configure(EntityTypeBuilder<WorkflowTransitionLog> builder)
        {
            builder.Property(p => p.UserData).HasJsonConversion();
        }
    }
}
