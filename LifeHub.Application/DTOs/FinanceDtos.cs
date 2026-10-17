namespace LifeHub.Application.DTOs;

public sealed record FinanceCategoryResponse(string Id, string Label, string Type);
public sealed record FinanceTransactionRequest(string Type, decimal Amount, string Category, string? Note, string Date);
public sealed record FinanceTransactionResponse(Guid Id, string Type, decimal Amount, string Category, string? Note, string Date);
public sealed record FinanceBudgetRequest(string Month, string Category, decimal LimitAmount);
public sealed record FinanceBudgetResponse(string Category, decimal LimitAmount, decimal SpentAmount);
public sealed record FinanceOverviewResponse(
    string Month,
    string Currency,
    IReadOnlyList<FinanceCategoryResponse> Categories,
    decimal IncomeTotal,
    decimal ExpenseTotal,
    IReadOnlyList<FinanceTransactionResponse> Transactions,
    IReadOnlyList<FinanceBudgetResponse> Budgets);
