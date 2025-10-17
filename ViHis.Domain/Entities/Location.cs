
using ViHis.Domain.Common;
namespace ViHis.Domain.Entities;
public class Location : BaseEntity
{
    public string Slug { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ParentId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? GeoJson { get; set; }
    public string? Summary { get; set; }
}
