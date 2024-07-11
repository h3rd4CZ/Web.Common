namespace RhDev.Common.Web.Core.DataAccess.SQL
{
    public class PaginatedResult<TEntity>
    {
        public PaginatedResult(int totalItems, IList<TEntity> data)
        {
            Data = data;
            TotalItems = totalItems;
        }
        public int TotalItems { get; }
        public IList<TEntity> Data { get; } = default!;
    }
}
