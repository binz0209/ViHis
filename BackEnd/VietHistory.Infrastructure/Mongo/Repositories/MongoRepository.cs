using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VietHistory.Domain.Common;

namespace VietHistory.Infrastructure.Mongo.Repositories;

/// <summary>
/// Generic repository cho MongoDB (Entity kế thừa BaseEntity)
/// </summary>
public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly IMongoCollection<T> _col;

    public MongoRepository(IMongoDatabase db, string collectionName)
    {
        _col = db.GetCollection<T>(collectionName);
    }

    public MongoRepository(IMongoCollection<T> collection)
    {
        _col = collection;
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _col.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int take = 50)
    {
        return await _col.Find(predicate).Skip(skip).Limit(take).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _col.InsertOneAsync(entity);
        return entity;
    }

    public async Task AddManyAsync(IEnumerable<T> entities)
    {
        await _col.InsertManyAsync(entities);
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var res = await _col.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return res.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var res = await _col.DeleteOneAsync(x => x.Id == id);
        return res.DeletedCount > 0;
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _col.CountDocumentsAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        var count = await _col.CountDocumentsAsync(predicate, new CountOptions { Limit = 1 });
        return count > 0;
    }

    public IMongoCollection<T> Collection => _col;
}

