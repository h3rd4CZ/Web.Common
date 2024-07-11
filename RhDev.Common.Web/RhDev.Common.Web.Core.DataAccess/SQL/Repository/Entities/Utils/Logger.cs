using System.ComponentModel.DataAnnotations;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Utils;

public class Logger : StoreEntity, IDataStoreEntity
{
    public int Id { get; set; }
    [MaxLength(4096)]
    public string? Message { get; set; }
    [MaxLength(4096)]
    public string? MessageTemplate { get; set; }
    [MaxLength(64)]
    public string Level { get; set; } = default!;

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    [MaxLength(32768)]
    public string? Exception { get; set; }
    [MaxLength(64)]
    public string? UserName { get; set; }
    [MaxLength(64)]
    public string? ClientIP { get; set; }
    [MaxLength(1024)]
    public string? ClientAgent { get; set; }
    [MaxLength(32768)]
    public string? Properties { get; set; }
    [MaxLength(32768)]
    public string? LogEvent { get; set; }
    [MaxLength(64)]
    public string? CorrelationId { get; set; }
    [MaxLength(1024)]
    public string? SourceContext { get; set; }
    [MaxLength(64)]
    public string? AppIdentifier { get; set; }
    public override string Identifier => Id.ToString();
}

