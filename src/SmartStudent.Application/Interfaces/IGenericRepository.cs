using System.Linq.Expressions;
using SmartStudent.Application.Common;
using SmartStudent.Core.Common;

namespace SmartStudent.Application.Interfaces;

/// <summary>
/// Generic repository contract supporting AsNoTracking by default for high-performance reads.
/// </summary>
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(
        Guid id,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<PaginatedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void SoftDelete(T entity);
}
