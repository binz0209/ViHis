
using ViHis.Domain.Common;
namespace ViHis.Domain.Entities;
public class User : BaseEntity
{
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? DisplayName { get; set; }
    public List<string> RoleNames { get; set; } = new();
}
