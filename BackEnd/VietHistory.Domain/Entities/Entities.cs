using System;
using MongoDB.Bson.Serialization.Attributes;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Entities;

public class Period : BaseEntity
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("startYear")] public int? StartYear { get; set; }
    [BsonElement("endYear")] public int? EndYear { get; set; }
    [BsonElement("description")] public string? Description { get; set; }
}

public class Dynasty : BaseEntity
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("founderId")] public string? FounderId { get; set; }
    [BsonElement("startYear")] public int? StartYear { get; set; }
    [BsonElement("endYear")] public int? EndYear { get; set; }
    [BsonElement("capital")] public string? Capital { get; set; }
    [BsonElement("description")] public string? Description { get; set; }
}

public class Person : BaseEntity
{
    [BsonElement("fullName")] public string FullName { get; set; } = string.Empty;
    [BsonElement("bornYear")] public int? BornYear { get; set; }
    [BsonElement("diedYear")] public int? DiedYear { get; set; }
    [BsonElement("dynastyId")] public string? DynastyId { get; set; }
    [BsonElement("roles")] public List<string> Roles { get; set; } = new();
    [BsonElement("summary")] public string? Summary { get; set; }
}

public class Battle : BaseEntity
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("year")] public int? Year { get; set; }
    [BsonElement("placeId")] public string? PlaceId { get; set; }
    [BsonElement("description")] public string? Description { get; set; }
}

public class Place : BaseEntity
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("lat")] public double? Lat { get; set; }
    [BsonElement("lng")] public double? Lng { get; set; }
    [BsonElement("province")] public string? Province { get; set; }
    [BsonElement("country")] public string? Country { get; set; }
}

public class Event : BaseEntity
{
    [BsonElement("title")] public string Title { get; set; } = string.Empty;
    [BsonElement("summary")] public string? Summary { get; set; }
    [BsonElement("year")] public int? Year { get; set; }
    [BsonElement("month")] public int? Month { get; set; }
    [BsonElement("day")] public int? Day { get; set; }
    [BsonElement("peopleIds")] public List<string> PeopleIds { get; set; } = new();
    [BsonElement("dynastyIds")] public List<string> DynastyIds { get; set; } = new();
    [BsonElement("placeIds")] public List<string> PlaceIds { get; set; } = new();
    [BsonElement("sourceIds")] public List<string> SourceIds { get; set; } = new();
}

public class Source : BaseEntity
{
    [BsonElement("type")] public string Type { get; set; } = "web";
    [BsonElement("title")] public string Title { get; set; } = string.Empty;
    [BsonElement("url")] public string? Url { get; set; }
}

public class Media : BaseEntity
{
    [BsonElement("type")] public string Type { get; set; } = "image";
    [BsonElement("url")] public string Url { get; set; } = string.Empty;
    [BsonElement("caption")] public string? Caption { get; set; }
    [BsonElement("entityType")] public string EntityType { get; set; } = string.Empty; // e.g., Person, Event
    [BsonElement("entityId")] public string EntityId { get; set; } = string.Empty;
}

public class AppUser : BaseEntity
{
    [BsonElement("username")] public string Username { get; set; } = string.Empty;
    [BsonElement("email")] public string Email { get; set; } = string.Empty;
    [BsonElement("passwordHash")] public string PasswordHash { get; set; } = string.Empty;
    [BsonElement("roleIds")] public List<string> RoleIds { get; set; } = new();
}

public class AppRole : BaseEntity
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("permissions")] public List<string> Permissions { get; set; } = new();
}

public class Bookmark : BaseEntity
{
    [BsonElement("userId")] public string UserId { get; set; } = string.Empty;
    [BsonElement("entityType")] public string EntityType { get; set; } = string.Empty;
    [BsonElement("entityId")] public string EntityId { get; set; } = string.Empty;
}

public class AuditLog : BaseEntity
{
    [BsonElement("userId")] public string? UserId { get; set; }
    [BsonElement("action")] public string Action { get; set; } = string.Empty;
    [BsonElement("meta")] public Dictionary<string, string> Meta { get; set; } = new();
}

public class ChatMessage : BaseEntity
{
    [BsonElement("text")] public string Text { get; set; } = string.Empty;
    [BsonElement("sender")] public string Sender { get; set; } = string.Empty; // "user" or "assistant"
    [BsonElement("chatId")] public string ChatId { get; set; } = string.Empty;
}

public class ChatHistory : BaseEntity
{
    [BsonElement("machineId")] public string MachineId { get; set; } = string.Empty;
    [BsonElement("userId")] public string? UserId { get; set; }
    [BsonElement("name")] public string Name { get; set; } = "Chat";
    [BsonElement("lastMessageAt")] public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    [BsonElement("messageIds")] public List<string> MessageIds { get; set; } = new();
}
