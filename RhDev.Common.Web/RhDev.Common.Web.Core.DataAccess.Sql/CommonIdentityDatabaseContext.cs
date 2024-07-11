using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.Sql.Extensions;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Utils;

namespace RhDev.Common.Web.Core.DataAccess.Sql
{
    public class CommonIdentityDatabaseContext : CommonIdentityDatabaseContext<IdentityUser, IdentityRole, string>
    {
        public CommonIdentityDatabaseContext() { }

        public CommonIdentityDatabaseContext(DbContextOptions contextOptions) : base(contextOptions) { }
    }

    public class CommonIdentityDatabaseContext<TUser> : CommonIdentityDatabaseContext<TUser, IdentityRole, string> where TUser : IdentityUser
    {
        public CommonIdentityDatabaseContext() { }

        public CommonIdentityDatabaseContext(DbContextOptions contextOptions) : base(contextOptions) { }
    }

    public class CommonIdentityDatabaseContext<TUser, TRole, TKey> : CommonIdentityDatabaseContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    {
        public CommonIdentityDatabaseContext() { }

        public CommonIdentityDatabaseContext(DbContextOptions contextOptions) : base(contextOptions) { }
    }

    public class CommonDatabaseContext : DbContext
    {
        public DbSet<DayOff> DayOffs { get; set; }
        public DbSet<ApplicationUserSettings> ApplicationUserSettings { get; set; }
        public DbSet<Logger> Loggers { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }

        public CommonDatabaseContext() { }

        public CommonDatabaseContext(DbContextOptions contextOptions) : base(contextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureCommonDatabaseModelBuilder();
        }
    }   

    public class CommonIdentityDatabaseContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
        /// <summary>
        /// For tests only
        /// </summary>
        public CommonIdentityDatabaseContext() { }

        public CommonIdentityDatabaseContext(DbContextOptions contextOptions) : base(contextOptions) { }
                
        public DbSet<DayOff> DayOffs { get; set; }
        public DbSet<ApplicationUserSettings> ApplicationUserSettings { get; set; }
        public DbSet<Logger> Loggers { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureCommonDatabaseModelBuilder();
        }
    }
}