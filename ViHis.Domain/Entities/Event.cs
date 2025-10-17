
using ViHis.Domain.Common;
namespace ViHis.Domain.Entities;
public class Event : BaseEntity
{
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string EventTypeId { get; set; } = default!;
    public string? PeriodId { get; set; }
    public string? DynastyId { get; set; }
    public string? Summary { get; set; }
    public DateParts Start { get; set; } = new();
    public DateParts End { get; set; } = new();
    public string? Result { get; set; }
    public string? BattleId { get; set; }
    public List<string> LocationIds { get; set; } = new();
    public List<string> SideNames { get; set; } = new();
    public List<string> ParticipantIds { get; set; } = new();
    public List<string> TagIds { get; set; } = new();
}
