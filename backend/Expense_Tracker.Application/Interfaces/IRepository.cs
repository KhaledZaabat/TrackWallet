using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common;
using System.Linq.Expressions;

namespace Expense_Tracker.Application.Interfaces;

public interface IRepository<T> where T : Entity
{
    /// <summary>
    /// Untracked queryable for reads. Always use this unless mutating.
    /// </summary>
    IQueryable<T> Query();

    /// <summary>
    /// Tracked queryable. Use ONLY when you intend to Update/Delete.
    /// </summary>
    IQueryable<T> QueryTracked();

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
