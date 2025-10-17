
using ViHis.Domain.Common;
namespace ViHis.Domain.Entities;
public class Person : BaseEntity
{
    public string Slug { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? AltName { get; set; }
    public DateParts Birth { get; set; } = new();
    public DateParts Death { get; set; } = new();
    public string? PeriodId { get; set; }
    public string? DynastyId { get; set; }
    public string? Summary { get; set; }
    public List<string> Aliases { get; set; } = new();
    public List<string> Titles { get; set; } = new();
    public List<string> TagIds { get; set; } = new();
}
