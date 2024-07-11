using Microsoft.AspNetCore.Identity;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common
{
    public interface IAuditTrailEntity<TUSer> : IDataStoreEntity where TUSer : IdentityUser
    {
        DateTime? Created { get; set; }

        public TUSer? CreatedBy { get; set; }
        string? CreatedById { get; set; }

        DateTime? LastModified { get; set; }

        public TUSer? LastModifiedBy { get; set; }
        string? LastModifiedById { get; set; }
    }
}
