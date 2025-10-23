using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Domain.Entities;
using VietHistory.Application.DTOs.Ingest;

namespace VietHistory.Infrastructure.Mongo
{
    /// <summary>
    /// Lớp MongoContext — giữ kết nối và các collection MongoDB
    /// </summary>
    public sealed class MongoContext
    {
        public IMongoDatabase Db { get; }

        // ===== Các collection chính trong hệ thống =====
        public IMongoCollection<Period> Periods => Db.GetCollection<Period>("periods");
        public IMongoCollection<Dynasty> Dynasties => Db.GetCollection<Dynasty>("dynasties");
        public IMongoCollection<Person> People => Db.GetCollection<Person>("people");
        public IMongoCollection<Event> Events => Db.GetCollection<Event>("events");
        public IMongoCollection<Place> Places => Db.GetCollection<Place>("places");
        public IMongoCollection<Battle> Battles => Db.GetCollection<Battle>("battles");
        public IMongoCollection<Source> SourcesLegacy => Db.GetCollection<Source>("sources_legacy"); // dữ liệu cũ (Entity)
        public IMongoCollection<Media> Media => Db.GetCollection<Media>("media");
        public IMongoCollection<AppUser> Users => Db.GetCollection<AppUser>("users");
        public IMongoCollection<AppRole> Roles => Db.GetCollection<AppRole>("roles");
        public IMongoCollection<Bookmark> Bookmarks => Db.GetCollection<Bookmark>("bookmarks");
        public IMongoCollection<AuditLog> AuditLogs => Db.GetCollection<AuditLog>("auditLogs");

        // ===== Các collection phục vụ AI ingest (PDFs, fine-tune data) =====
        public IMongoCollection<SourceDoc> Sources => Db.GetCollection<SourceDoc>("sources");
        public IMongoCollection<ChunkDoc> Chunks => Db.GetCollection<ChunkDoc>("chunks");

        public MongoContext(MongoSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ArgumentNullException(nameof(settings.ConnectionString), "MongoDB connection string is missing.");

            if (string.IsNullOrWhiteSpace(settings.Database))
                throw new ArgumentNullException(nameof(settings.Database), "Database name missing in config.");

            var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            clientSettings.ConnectTimeout = TimeSpan.FromSeconds(10);

            var client = new MongoClient(clientSettings);
            Db = client.GetDatabase(settings.Database);
        }

        /// <summary>
        /// Ping để test kết nối MongoDB
        /// </summary>
        public async Task<bool> PingAsync()
        {
            try
            {
                var result = await Db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                return result.Contains("ok") && result["ok"].ToDouble() == 1.0;
            }
            catch
            {
                return false;
            }
        }
    }
}
