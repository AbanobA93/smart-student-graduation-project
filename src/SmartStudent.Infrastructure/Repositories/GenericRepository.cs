using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartStudent.Application.Common;
using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Common;
using SmartStudent.Infrastructure.Data;

namespace SmartStudent.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using EF Core with AsNoTracking enabled by default on read operations.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(
        Guid id,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy is not null)
        {
            query = orderBy(query);
        }
        else
        {
            query = query.OrderByDescending(e => e.CreatedAt);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        return predicate is null
            ? await query.CountAsync(cancellationToken)
            : await query.CountAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }
}
