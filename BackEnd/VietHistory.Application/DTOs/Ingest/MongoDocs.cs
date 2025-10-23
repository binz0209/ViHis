using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VietHistory.Application.DTOs.Ingest
{
    public sealed class SourceDoc
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Title { get; set; } = default!;
        public string? Author { get; set; }
        public int? Year { get; set; }
        public string FileName { get; set; } = default!;
        public int Pages { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public sealed class ChunkDoc
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonRepresentation(BsonType.ObjectId)]
        public string SourceId { get; set; } = default!;
        public int ChunkIndex { get; set; }
        public string Content { get; set; } = default!;
        public int PageFrom { get; set; }
        public int PageTo { get; set; }
        public int ApproxTokens { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
