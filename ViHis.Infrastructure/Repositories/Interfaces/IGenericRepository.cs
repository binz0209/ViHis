
using System.Linq.Expressions;
namespace ViHis.Infrastructure.Repositories.Interfaces;
public interface IGenericRepository<T>
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(string id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
}
