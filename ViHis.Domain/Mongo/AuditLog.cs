
namespace ViHis.Domain.Mongo;
public class AuditLog
{
    public string Id { get; set; } = default!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Actor { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string Entity { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string? Payload { get; set; }
}
