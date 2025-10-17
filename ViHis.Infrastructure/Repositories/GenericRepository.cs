
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;
using ViHis.Infrastructure.Mongo;
using ViHis.Infrastructure.Repositories.Interfaces;

namespace ViHis.Infrastructure.Repositories;
public class GenericRepository<T> : IGenericRepository<T>
{
    protected readonly IMongoCollection<T> _col;
    public GenericRepository(IMongoCollection<T> col) { _col = col; }
    public IQueryable<T> Query() => _col.AsQueryable();
    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        return await _col.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<IReadOnlyList<T>> GetAllAsync() => await _col.AsQueryable().ToListAsync();
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _col.AsQueryable().Where(predicate).ToListAsync();
    public async Task<T> AddAsync(T entity) { await _col.InsertOneAsync(entity); return entity; }
    public async Task UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        await _col.ReplaceOneAsync(filter, entity);
    }
    public async Task DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
        await _col.DeleteOneAsync(filter);
    }
}
