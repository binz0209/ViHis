using MongoDB.Driver;
using VietHistory.Domain.Entities;

namespace VietHistory.Infrastructure.Mongo;

/// <summary>
/// Interface for MongoContext - enables mocking in unit tests
/// Created for testing purposes to work around sealed class limitation
/// </summary>
public interface IMongoContext
{
    IMongoDatabase Db { get; }
    
    // Main collections
    IMongoCollection<AppUser> Users { get; }
    IMongoCollection<AppRole> Roles { get; }
    
    // AI/RAG collections
    IMongoCollection<SourceDoc> Sources { get; }
    IMongoCollection<ChunkDoc> Chunks { get; }
    
    // Chat History collections
    IMongoCollection<ChatHistory> ChatHistories { get; }
    IMongoCollection<ChatMessage> ChatMessages { get; }
    
    // Quiz collections
    IMongoCollection<Quiz> Quizzes { get; }
    IMongoCollection<QuizAttempt> QuizAttempts { get; }
    
    Task<bool> PingAsync();
}

