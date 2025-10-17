
namespace ViHis.Domain.Entities;
public class Battle
{
    public string Id { get; set; } = default!;
    public string EventId { get; set; } = default!;
    public string? Outcome { get; set; }
    public int? StrengthSideA { get; set; }
    public int? StrengthSideB { get; set; }
    public int? CasualtiesSideA { get; set; }
    public int? CasualtiesSideB { get; set; }
    public string? Terrain { get; set; }
}
