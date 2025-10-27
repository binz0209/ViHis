using MongoDB.Driver;
using VietHistory.Domain.Entities;
using VietHistory.Application.DTOs.Ingest;

namespace VietHistory.Infrastructure.Mongo;

/// <summary>
/// Interface for MongoContext - enables mocking in unit tests
/// Created for testing purposes to work around sealed class limitation
/// </summary>
public interface IMongoContext
{
    IMongoDatabase Db { get; }
    
    // Main collections
    IMongoCollection<Period> Periods { get; }
    IMongoCollection<Dynasty> Dynasties { get; }
    IMongoCollection<Person> People { get; }
    IMongoCollection<Event> Events { get; }
    IMongoCollection<Place> Places { get; }
    IMongoCollection<Battle> Battles { get; }
    IMongoCollection<Source> SourcesLegacy { get; }
    IMongoCollection<Media> Media { get; }
    IMongoCollection<AppUser> Users { get; }
    IMongoCollection<AppRole> Roles { get; }
    IMongoCollection<Bookmark> Bookmarks { get; }
    IMongoCollection<AuditLog> AuditLogs { get; }
    
    // AI/RAG collections
    IMongoCollection<SourceDoc> Sources { get; }
    IMongoCollection<ChunkDoc> Chunks { get; }
    
    // Chat History collections
    IMongoCollection<ChatHistory> ChatHistories { get; }
    IMongoCollection<ChatMessage> ChatMessages { get; }
    
    // Quiz collections
    IMongoCollection<Quiz> Quizzes { get; }
    IMongoCollection<QuizQuestion> QuizQuestions { get; }
    IMongoCollection<QuizAttempt> QuizAttempts { get; }
    
    Task<bool> PingAsync();
}

