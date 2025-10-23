using System.Linq.Expressions;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Common;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int take = 50);
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<long> CountAsync(Expression<Func<T, bool>> predicate);
}
