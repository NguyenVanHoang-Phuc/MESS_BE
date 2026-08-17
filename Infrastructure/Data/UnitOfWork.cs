using MESS.Domain.Interfaces;

namespace MESS.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly MessDbContext _context;

    public UnitOfWork(MessDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
