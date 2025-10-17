
namespace ViHis.Domain.Common;
public class DateParts
{
    public int? Year { get; set; }
    public byte? Month { get; set; }
    public byte? Day { get; set; }
    public bool IsBCE { get; set; } = false;
}
