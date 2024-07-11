namespace RhDev.Common.Web.Core.DataAccess.SQL
{
    public interface ISoftDelete
    {
        DateTime? Deleted { get; set; }
        string? DeletedBy { get; set; }
    }
}
