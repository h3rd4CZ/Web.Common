using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace RhDev.Common.Web.Core.DataAccess.Sql.Extensions
{    

    public static class ModelBuilderExtension
    {
        public static void ApplyGlobalFilters<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> expression)
        {
            var entities = modelBuilder.Model
                .GetEntityTypes()
                .Where(e => e.ClrType.GetInterface(typeof(TInterface).Name) != null)
                .Select(e => e.ClrType);
            foreach (var entity in entities)
            {
                var newParam = Expression.Parameter(entity);
                var newBody = ReplacingExpressionVisitor.Replace(expression.Parameters.Single(), newParam, expression.Body);
                modelBuilder.Entity(entity).HasQueryFilter(Expression.Lambda(newBody, newParam));
            }
        }

        public static void ConfigureCommonDatabaseModelBuilder(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.ApplyGlobalFilters<ISoftDelete>(s => s.Deleted == null);

            modelBuilder.Entity<DayOff>()
                           .HasData(
                           new DayOff { Id = 1, Title = "Den obnovy samostatného českého státu", Day = new DateTime(2022, 1, 1), Repeat = true },
                           new DayOff { Id = 2, Title = "Svátek práce", Day = new DateTime(2022, 5, 1), Repeat = true },
                           new DayOff { Id = 3, Title = "Den vítězství", Day = new DateTime(2022, 5, 8), Repeat = true },
                           new DayOff { Id = 5, Title = "Den slovanských věrozvěstů Cyrila a Metoděje", Day = new DateTime(2022, 7, 5), Repeat = true },
                           new DayOff { Id = 6, Title = "Den upálení mistra Jana Husa", Day = new DateTime(2022, 7, 6), Repeat = true },
                           new DayOff { Id = 7, Title = "Den české státnosti", Day = new DateTime(2022, 9, 28), Repeat = true },
                           new DayOff { Id = 8, Title = "Den boje za svobodu a demokracii", Day = new DateTime(2022, 11, 17), Repeat = true },
                           new DayOff { Id = 9, Title = "Štědrý den", Day = new DateTime(2022, 12, 24), Repeat = true },
                           new DayOff { Id = 10, Title = "1. svátek vánoční", Day = new DateTime(2022, 12, 25), Repeat = true },
                           new DayOff { Id = 11, Title = "2. svátek vánoční", Day = new DateTime(2022, 12, 26), Repeat = true },
                           new DayOff { Id = 12, Title = "2. svátek vánoční", Day = new DateTime(2022, 12, 26), Repeat = true },
                           new DayOff { Id = 13, Title = "Velký pátek", Day = new DateTime(2023, 4, 7) },
                           new DayOff { Id = 14, Title = "Velký pátek", Day = new DateTime(2024, 3, 29) },
                           new DayOff { Id = 15, Title = "Velký pátek", Day = new DateTime(2025, 4, 18) },
                           new DayOff { Id = 16, Title = "Velký pátek", Day = new DateTime(2026, 4, 3) },
                           new DayOff { Id = 17, Title = "Velký pátek", Day = new DateTime(2027, 3, 26) },
                           new DayOff { Id = 18, Title = "Velký pátek", Day = new DateTime(2028, 4, 14) },
                           new DayOff { Id = 19, Title = "Velký pátek", Day = new DateTime(2029, 3, 30) },
                           new DayOff { Id = 20, Title = "Velký pátek", Day = new DateTime(2030, 4, 19) },
                           new DayOff { Id = 21, Title = "Velikonoční pondělí", Day = new DateTime(2023, 4, 10) },
                           new DayOff { Id = 22, Title = "Velikonoční pondělí", Day = new DateTime(2024, 4, 1) },
                           new DayOff { Id = 23, Title = "Velikonoční pondělí", Day = new DateTime(2025, 4, 21) },
                           new DayOff { Id = 24, Title = "Velikonoční pondělí", Day = new DateTime(2026, 4, 6) },
                           new DayOff { Id = 25, Title = "Velikonoční pondělí", Day = new DateTime(2027, 3, 29) },
                           new DayOff { Id = 26, Title = "Velikonoční pondělí", Day = new DateTime(2028, 4, 17) },
                           new DayOff { Id = 27, Title = "Velikonoční pondělí", Day = new DateTime(2029, 4, 2) },
                           new DayOff { Id = 28, Title = "Velikonoční pondělí", Day = new DateTime(2030, 4, 22) }
                           );
        }
    }
}
