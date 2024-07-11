using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.Sql;

namespace RhDev.Customer.Component.Core.Impl.Data
{
    public class ApplicationDbContext : CommonIdentityDatabaseContext<ApplicationUser>
    {
        public DbSet<City> Cities { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>()
               .HasOne(b => b.CreatedBy)
               .WithMany(a => a.AuthoredCities)
               .HasForeignKey(c => c.CreatedById)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<City>()
               .HasOne(b => b.LastModifiedBy)
               .WithMany(a => a.ModifiedCities)
               .HasForeignKey(c => c.LastModifiedById)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApplicationUser>()
               .HasOne(b => b.CreatedBy)
               .WithMany(a => a.AuthoredUsers)
               .HasForeignKey(c => c.CreatedById)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApplicationUser>()
               .HasOne(b => b.LastModifiedBy)
               .WithMany(a => a.ModifiedUsers)
               .HasForeignKey(c => c.LastModifiedById)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}