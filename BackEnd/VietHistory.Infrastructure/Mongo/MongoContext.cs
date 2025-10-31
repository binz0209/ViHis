using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Domain.Entities;

namespace VietHistory.Infrastructure.Mongo
{
    /// <summary>
    /// Lớp MongoContext — giữ kết nối và các collection MongoDB
    /// </summary>
    public sealed class MongoContext : IMongoContext
    {
        public IMongoDatabase Db { get; }

        // ===== Các collection chính trong hệ thống =====
        public IMongoCollection<AppUser> Users => Db.GetCollection<AppUser>("users");
        public IMongoCollection<AppRole> Roles => Db.GetCollection<AppRole>("roles");

        // ===== Các collection phục vụ AI ingest (PDFs, fine-tune data) =====
        public IMongoCollection<SourceDoc> Sources => Db.GetCollection<SourceDoc>("sources");
        public IMongoCollection<ChunkDoc> Chunks => Db.GetCollection<ChunkDoc>("chunks");

        // ===== Chat History Collections =====
        public IMongoCollection<ChatHistory> ChatHistories => Db.GetCollection<ChatHistory>("chatHistories");
        public IMongoCollection<ChatMessage> ChatMessages => Db.GetCollection<ChatMessage>("chatMessages");

        // ===== Quiz Collections =====
        public IMongoCollection<Quiz> Quizzes => Db.GetCollection<Quiz>("quizzes");
        public IMongoCollection<QuizAttempt> QuizAttempts => Db.GetCollection<QuizAttempt>("quizAttempts");

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
