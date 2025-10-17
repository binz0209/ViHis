
using ViHis.Domain.Common;
namespace ViHis.Domain.Entities;
public class Period : BaseEntity
{
    public string Slug { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Summary { get; set; }
    public DateParts Start { get; set; } = new();
    public DateParts End { get; set; } = new();
}
