using MESS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using IDbTransaction = MESS.Domain.Interfaces.IDbTransaction;

namespace MESS.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly MessDbContext _context;

    public UnitOfWork(MessDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var efTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransactionWrapper(efTransaction);
    }

    public void ClearChangeTracker()
        => _context.ChangeTracker.Clear();

    public void Dispose()
        => _context.Dispose();
}

// Wrapper — ẩn IDbContextTransaction của EF Core
internal class EfTransactionWrapper : IDbTransaction
{
    private readonly IDbContextTransaction _inner;

    public EfTransactionWrapper(IDbContextTransaction inner)
    {
        _inner = inner;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
        => await _inner.CommitAsync(cancellationToken);

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
        => await _inner.RollbackAsync(cancellationToken);

    public async ValueTask DisposeAsync()
        => await _inner.DisposeAsync();
}
