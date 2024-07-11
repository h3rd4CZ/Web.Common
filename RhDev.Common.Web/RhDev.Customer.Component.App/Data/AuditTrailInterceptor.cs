using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Common;
using System;
using RhDev.Common.Web.Core;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Customer.Component.Core.Impl.Data;

namespace RhDev.Customer.Component.App.Data
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICentralClockProvider centralClockProvider;
        private List<AuditTrail> _temporaryAuditTrailList = new();
        public AuditableEntityInterceptor(
            IHttpContextAccessor httpContextAccessor,
            ICentralClockProvider centralClockProvider)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.centralClockProvider = centralClockProvider;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {

            UpdateEntities(eventData.Context!);
            _temporaryAuditTrailList = TryInsertTemporaryAuditTrail(eventData.Context!, cancellationToken);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            var resultValueTask = await base.SavedChangesAsync(eventData, result, cancellationToken);
            await TryUpdateTemporaryPropertiesForAuditTrail(eventData.Context!, cancellationToken).ConfigureAwait(false);
            return resultValueTask;
        }
        private void UpdateEntities(DbContext context)
        {
            var userId = GetCurrentUserId() ??  "{A1A7BF4D-DD01-4E4E-A8FC-5008022B00BB}";

            var now = centralClockProvider.Now().ExportDateTime;

            foreach (var entry in context.ChangeTracker.Entries<IAuditTrailEntity<ApplicationUser>>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedById = userId;
                        entry.Entity.Created = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedById = userId;
                        entry.Entity.LastModified = now;
                        break;
                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete softDelete)
                        {
                            softDelete.DeletedBy = userId;
                            softDelete.Deleted = now;
                            entry.State = EntityState.Deleted;
                        }
                        break;
                    case EntityState.Unchanged:
                        if (entry.HasChangedOwnedEntities())
                        {
                            entry.Entity.LastModifiedById = userId;
                            entry.Entity.LastModified = now;
                        }
                        break;
                }
            }
        }

        private List<AuditTrail> TryInsertTemporaryAuditTrail(DbContext context, CancellationToken cancellationToken = default)
        {

            var userId = GetCurrentUserId();

            var now = centralClockProvider.Now().ExportDateTime;

            context.ChangeTracker.DetectChanges();
            var temporaryAuditEntries = new List<AuditTrail>();
            foreach (var entry in context.ChangeTracker.Entries<IAuditTrailEntity<ApplicationUser>>())
            {
                if (entry.Entity is AuditTrail ||
                    entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditTrail()
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = userId,
                    DateTime = now,
                    AffectedColumns = new List<string>(),
                    NewValues = new(),
                    OldValues = new(),
                };
                foreach (var property in entry.Properties)
                {

                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey() && property.CurrentValue is not null)
                    {
                        auditEntry.PrimaryKey[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            if (property.CurrentValue is not null)
                            {
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            if (property.OriginalValue is not null)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                            }
                            break;

                        case EntityState.Modified when property.IsModified && ((property.OriginalValue is null && property.CurrentValue is not null) || (property.OriginalValue is not null && property.OriginalValue.Equals(property.CurrentValue) == false)):
                            auditEntry.AffectedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Update;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            if (property.CurrentValue is not null)
                            {
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;

                    }
                }
                temporaryAuditEntries.Add(auditEntry);
            }
            return temporaryAuditEntries;
        }

        private async Task TryUpdateTemporaryPropertiesForAuditTrail(DbContext context, CancellationToken cancellationToken = default)
        {
            if (_temporaryAuditTrailList.Any())
            {
                foreach (var auditEntry in _temporaryAuditTrailList)
                {
                    foreach (var prop in auditEntry.TemporaryProperties)
                    {
                        if (prop.Metadata.IsPrimaryKey() && prop.CurrentValue is not null)
                        {
                            auditEntry.PrimaryKey[prop.Metadata.Name] = prop.CurrentValue;
                        }
                        else if (auditEntry.NewValues is not null && prop.CurrentValue is not null)
                        {
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }

                }
                await context.AddRangeAsync(_temporaryAuditTrailList);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _temporaryAuditTrailList.Clear();
            }
        }

        private string? GetCurrentUserId()
        {
            var currentUser = httpContextAccessor.HttpContext?.User;

            if (currentUser is not null) return currentUser?.Identity?.Name;

            return string.Empty;
        }
    }

    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
