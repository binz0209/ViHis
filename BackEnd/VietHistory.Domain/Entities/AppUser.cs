using MongoDB.Bson.Serialization.Attributes;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Entities;

public class AppUser : BaseEntity
{
    [BsonElement("username")] public string Username { get; set; } = string.Empty;
    [BsonElement("email")] public string Email { get; set; } = string.Empty;
    [BsonElement("passwordHash")] public string PasswordHash { get; set; } = string.Empty;
    [BsonElement("roleIds")] public List<string> RoleIds { get; set; } = new();
}

