using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using AspNetCoreStarterKit.Domain.Interfaces;
using AspNetCoreStarterKit.Infrastructure.Persistence;

namespace AspNetCoreStarterKit.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate == null ? await _dbSet.FirstOrDefaultAsync(cancellationToken) : await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    public void Update(T entity) => _dbSet.Update(entity);
    public void UpdateRange(IEnumerable<T> entities) => _dbSet.UpdateRange(entities);
    public void Remove(T entity) => _dbSet.Remove(entity);
    public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate == null ? await _dbSet.CountAsync(cancellationToken) : await _dbSet.CountAsync(predicate, cancellationToken);
}