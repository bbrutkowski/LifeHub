using LifeHub.Domain.Entity;

namespace LifeHub.Application.Interfaces.Repositories;

public interface IFinanceRepository
{
    Task<IReadOnlyList<FinanceTransaction>> GetTransactions(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FinanceBudget>> GetBudgets(Guid userId, DateOnly month, CancellationToken cancellationToken = default);
    Task AddTransaction(FinanceTransaction transaction, CancellationToken cancellationToken = default);
    Task<bool> DeleteTransaction(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
    Task<FinanceBudget> UpsertBudget(FinanceBudget budget, CancellationToken cancellationToken = default);
}
