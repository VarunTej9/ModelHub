using Microsoft.EntityFrameworkCore;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;

namespace ModelHub.Infrastructure.Persistence;

public sealed class EfRepository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly ModelHubDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public EfRepository(ModelHubDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(
            new object?[] { id },
            cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }
}
