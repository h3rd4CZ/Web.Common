using Microsoft.EntityFrameworkCore.Diagnostics;

namespace RhDev.Customer.Component.App.Data
{
    public class TestDbContextInterceptor : SaveChangesInterceptor
    {
        
        public TestDbContextInterceptor()
        {

        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
