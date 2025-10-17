
namespace ViHis.Domain.Entities;
public class Tag
{
    public string Id { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Category { get; set; }
}
