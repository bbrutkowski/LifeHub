using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Domain.Entity;
using LifeHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Infrastructure.Repository;

public sealed class FinanceRepository(ApplicationDbContext db) : IFinanceRepository
{
    public async Task<IReadOnlyList<FinanceTransaction>> GetTransactions(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await db.FinanceTransactions
            .Where(x => x.UserId == userId && x.Date >= startDate && x.Date < endDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FinanceBudget>> GetBudgets(Guid userId, DateOnly month, CancellationToken cancellationToken = default) =>
        await db.FinanceBudgets
            .Where(x => x.UserId == userId && x.Month == month)
            .ToListAsync(cancellationToken);

    public async Task AddTransaction(FinanceTransaction transaction, CancellationToken cancellationToken = default)
    {
        await db.FinanceTransactions.AddAsync(transaction, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteTransaction(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await db.FinanceTransactions
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId, cancellationToken);
        if (transaction is null) return false;
        db.FinanceTransactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FinanceBudget> UpsertBudget(FinanceBudget budget, CancellationToken cancellationToken = default)
    {
        var existing = await db.FinanceBudgets.FirstOrDefaultAsync(
            x => x.UserId == budget.UserId && x.Month == budget.Month && x.Category == budget.Category,
            cancellationToken);
        if (existing is null)
        {
            await db.FinanceBudgets.AddAsync(budget, cancellationToken);
            existing = budget;
        }
        else
        {
            existing.UpdateLimit(budget.LimitAmount);
        }
        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
