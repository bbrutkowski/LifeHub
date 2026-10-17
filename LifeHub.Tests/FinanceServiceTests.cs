using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Services;
using LifeHub.Domain.Entity;

namespace LifeHub.Tests;

public class FinanceServiceTests
{
    [Fact]
    public async Task GetOverview_ReturnsMonthTotalsBudgetSpendAndCurrency()
    {
        var user = new User(Guid.NewGuid(), "jane", "jane@example.com", "hash");
        user.UpdateProfile("jane", "jane@example.com", "Europe/Warsaw", "Warsaw", "EUR", "DD.MM.YYYY", "monday");
        var transactions = new[]
        {
            new FinanceTransaction(user.Id, "income", 2500m, "salary", null, new DateOnly(2026, 10, 1)),
            new FinanceTransaction(user.Id, "expense", 120.50m, "groceries", "market", new DateOnly(2026, 10, 10))
        };
        var repository = new FakeFinanceRepository(transactions,
            [new FinanceBudget(user.Id, new DateOnly(2026, 10, 1), "groceries", 300m)]);
        var service = new FinanceService(repository, new FakeUserRepository(user));

        var result = await service.GetOverview(user.Id, "2026-10");

        Assert.Equal("EUR", result.Currency);
        Assert.Equal(2500m, result.IncomeTotal);
        Assert.Equal(120.50m, result.ExpenseTotal);
        Assert.Equal("2026-10-10", result.Transactions[0].Date);
        Assert.Equal(120.50m, Assert.Single(result.Budgets).SpentAmount);
    }

    [Fact]
    public async Task AddTransaction_RejectsMoreThanTwoDecimalPlaces()
    {
        var user = new User(Guid.NewGuid(), "jane", "jane@example.com", "hash");
        var service = new FinanceService(new FakeFinanceRepository([], []), new FakeUserRepository(user));

        var exception = await Assert.ThrowsAsync<FinanceValidationException>(() =>
            service.AddTransaction(user.Id, new FinanceTransactionRequest("expense", 1.001m, "groceries", null, "2026-10-17")));

        Assert.Equal("Amount must have no more than two decimal places.", exception.Message);
    }

    private sealed class FakeFinanceRepository(
        IReadOnlyList<FinanceTransaction> transactions,
        IReadOnlyList<FinanceBudget> budgets) : IFinanceRepository
    {
        public Task<IReadOnlyList<FinanceTransaction>> GetTransactions(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FinanceTransaction>>(transactions.Where(x => x.UserId == userId && x.Date >= startDate && x.Date < endDate).ToArray());

        public Task<IReadOnlyList<FinanceBudget>> GetBudgets(Guid userId, DateOnly month, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FinanceBudget>>(budgets.Where(x => x.UserId == userId && x.Month == month).ToArray());

        public Task AddTransaction(FinanceTransaction transaction, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> DeleteTransaction(Guid userId, Guid transactionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<FinanceBudget> UpsertBudget(FinanceBudget budget, CancellationToken cancellationToken = default) =>
            Task.FromResult(budget);
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetById(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(id == user.Id ? user : null);

        public Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task Add(User newUser, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Update(User updatedUser, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
