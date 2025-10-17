
using MongoDB.Driver;
using ViHis.Domain.Entities;
using ViHis.Domain.Mongo;

namespace ViHis.Infrastructure.Mongo;
public class MongoOptions
{
    public string ConnectionString { get; set; } = default!;
    public string Database { get; set; } = default!;
}

public interface IMongoDbContext
{
    IMongoCollection<Person> People { get; }
    IMongoCollection<Event> Events { get; }
    IMongoCollection<Period> Periods { get; }
    IMongoCollection<Dynasty> Dynasties { get; }
    IMongoCollection<Domain.Entities.Tag> Tags { get; }
    IMongoCollection<Media> Media { get; }
    IMongoCollection<Source> Sources { get; }
    IMongoCollection<User> Users { get; }
    IMongoCollection<AuditLog> AuditLogs { get; }
}

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _db;
    public MongoDbContext(MongoOptions options)
    {
        var client = new MongoClient(options.ConnectionString);
        _db = client.GetDatabase(options.Database);
    }

    public IMongoCollection<Person> People => _db.GetCollection<Person>("people");
    public IMongoCollection<Event> Events => _db.GetCollection<Event>("events");
    public IMongoCollection<Period> Periods => _db.GetCollection<Period>("periods");
    public IMongoCollection<Dynasty> Dynasties => _db.GetCollection<Dynasty>("dynasties");
    public IMongoCollection<Domain.Entities.Tag> Tags => _db.GetCollection<Domain.Entities.Tag>("tags");
    public IMongoCollection<Media> Media => _db.GetCollection<Media>("media");
    public IMongoCollection<Source> Sources => _db.GetCollection<Source>("sources");
    public IMongoCollection<User> Users => _db.GetCollection<User>("users");
    public IMongoCollection<AuditLog> AuditLogs => _db.GetCollection<AuditLog>("audit_logs");
}
