using Microsoft.EntityFrameworkCore;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.Utils;
using System;

namespace RhDev.Common.Web.Core.DataAccess.Sql
{
    public class DatabaseAccessRepository<TDatabase> : IDatabaseAccessRepository<TDatabase> where TDatabase : DbContext
    {
        private bool Disposed = false;

        private TDatabase _db;
        public TDatabase Database => _db;

        ~DatabaseAccessRepository()
        {
            Dispose(false);
        }

        public DatabaseAccessRepository(TDatabase db)
        {
            Guard.NotNull(db, nameof(db));

            this._db = db;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (!Equals(null, _db)) _db.Dispose();
                }
                Disposed = true;
            }
        }

    }
}
