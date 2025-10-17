
using ViHis.Domain.Common;
namespace ViHis.Domain.Entities;
public class Source : BaseEntity
{
    public string Title { get; set; } = default!;
    public string? Url { get; set; }
    public string? Citation { get; set; }
}
