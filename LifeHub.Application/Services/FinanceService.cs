using System.Globalization;
using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Domain.Entity;

namespace LifeHub.Application.Services;

public sealed class FinanceService(IFinanceRepository financeRepository, IUserRepository userRepository) : IFinanceService
{
    private static readonly FinanceCategoryResponse[] SupportedCategories =
    [
        new("salary", "Wynagrodzenie", "income"),
        new("other-income", "Inne przychody", "income"),
        new("housing", "Mieszkanie", "expense"),
        new("groceries", "Jedzenie", "expense"),
        new("transport", "Transport", "expense"),
        new("health", "Zdrowie", "expense"),
        new("leisure", "Rozrywka", "expense"),
        new("other", "Inne", "expense")
    ];

    public async Task<FinanceOverviewResponse> GetOverview(Guid userId, string month, CancellationToken cancellationToken = default)
    {
        var monthStart = ParseMonth(month);
        var monthEnd = monthStart.AddMonths(1);
        var transactions = await financeRepository.GetTransactions(userId, monthStart, monthEnd, cancellationToken);
        var budgets = await financeRepository.GetBudgets(userId, monthStart, cancellationToken);
        var user = await userRepository.GetById(userId, cancellationToken);

        var transactionResponses = transactions
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .Select(ToResponse)
            .ToArray();
        var budgetResponses = budgets
            .OrderBy(x => x.Category)
            .Select(x => new FinanceBudgetResponse(
                x.Category,
                x.LimitAmount,
                transactions.Where(t => t.Type == "expense" && t.Category == x.Category).Sum(t => t.Amount)))
            .ToArray();

        return new FinanceOverviewResponse(
            month,
            string.IsNullOrWhiteSpace(user?.Currency) ? "PLN" : user.Currency,
            SupportedCategories,
            transactions.Where(x => x.Type == "income").Sum(x => x.Amount),
            transactions.Where(x => x.Type == "expense").Sum(x => x.Amount),
            transactionResponses,
            budgetResponses);
    }

    public async Task<FinanceTransactionResponse> AddTransaction(Guid userId, FinanceTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new FinanceValidationException("Transaction data is required.");
        if (request.Type is not ("income" or "expense"))
            throw new FinanceValidationException("Type must be 'income' or 'expense'.");
        ValidateAmount(request.Amount, "Amount");
        ValidateCategory(request.Category, request.Type);
        if (!DateOnly.TryParseExact(request.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new FinanceValidationException("Date must be a valid date in YYYY-MM-DD format.");
        if (request.Note?.Length > 500)
            throw new FinanceValidationException("Note must be 500 characters or fewer.");

        var transaction = new FinanceTransaction(userId, request.Type, request.Amount, request.Category, request.Note, date);
        await financeRepository.AddTransaction(transaction, cancellationToken);
        return ToResponse(transaction);
    }

    public Task<bool> DeleteTransaction(Guid userId, Guid transactionId, CancellationToken cancellationToken = default) =>
        financeRepository.DeleteTransaction(userId, transactionId, cancellationToken);

    public async Task<FinanceBudgetResponse> UpsertBudget(Guid userId, FinanceBudgetRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new FinanceValidationException("Budget data is required.");
        var month = ParseMonth(request.Month);
        ValidateAmount(request.LimitAmount, "Limit amount");
        ValidateCategory(request.Category, "expense");

        var budget = await financeRepository.UpsertBudget(
            new FinanceBudget(userId, month, request.Category, request.LimitAmount), cancellationToken);
        var monthTransactions = await financeRepository.GetTransactions(userId, month, month.AddMonths(1), cancellationToken);
        var spent = monthTransactions
            .Where(x => x.Type == "expense" && x.Category == request.Category)
            .Sum(x => x.Amount);

        return new FinanceBudgetResponse(budget.Category, budget.LimitAmount, spent);
    }

    private static DateOnly ParseMonth(string? month)
    {
        if (month is null || month.Length != 7 ||
            !DateOnly.TryParseExact($"{month}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            throw new FinanceValidationException("Month must be a valid month in YYYY-MM format.");
        return parsed;
    }

    private static void ValidateAmount(decimal amount, string label)
    {
        if (amount <= 0)
            throw new FinanceValidationException($"{label} must be greater than zero.");
        if (decimal.Round(amount, 2) != amount)
            throw new FinanceValidationException($"{label} must have no more than two decimal places.");
        if (amount > 9999999999999999.99m)
            throw new FinanceValidationException($"{label} is too large.");
    }

    private static void ValidateCategory(string? category, string type)
    {
        if (!SupportedCategories.Any(x => x.Id == category && x.Type == type))
        {
            var categoryType = type == "expense" ? "expense" : "income";
            throw new FinanceValidationException($"Category must be a supported {categoryType} category.");
        }
    }

    private static FinanceTransactionResponse ToResponse(FinanceTransaction transaction) =>
        new(transaction.Id, transaction.Type, transaction.Amount, transaction.Category, transaction.Note, transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}

public sealed class FinanceValidationException(string message) : Exception(message);
