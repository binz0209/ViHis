
using ViHis.Domain.Common;
namespace ViHis.Domain.Entities;
public class Media : BaseEntity
{
    public string Url { get; set; } = default!;
    public string? Caption { get; set; }
    public string? Type { get; set; } // image, video, youtube
    public string? EventId { get; set; }
    public string? PersonId { get; set; }
}
