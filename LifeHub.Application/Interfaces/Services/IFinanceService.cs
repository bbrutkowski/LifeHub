using LifeHub.Application.DTOs;

namespace LifeHub.Application.Interfaces.Services;

public interface IFinanceService
{
    Task<FinanceOverviewResponse> GetOverview(Guid userId, string month, CancellationToken cancellationToken = default);
    Task<FinanceTransactionResponse> AddTransaction(Guid userId, FinanceTransactionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTransaction(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
    Task<FinanceBudgetResponse> UpsertBudget(Guid userId, FinanceBudgetRequest request, CancellationToken cancellationToken = default);
}
