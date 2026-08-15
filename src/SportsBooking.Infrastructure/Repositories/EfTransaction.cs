using Microsoft.EntityFrameworkCore.Storage;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.Infrastructure.Repositories;

public sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken ct = default)
        => _transaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default)
        => _transaction.RollbackAsync(ct);

    public ValueTask DisposeAsync()
        => _transaction.DisposeAsync();
}