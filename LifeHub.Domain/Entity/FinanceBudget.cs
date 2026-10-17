namespace LifeHub.Domain.Entity;

public class FinanceBudget
{
    public Guid UserId { get; private set; }
    public DateOnly Month { get; private set; }
    public string Category { get; private set; } = null!;
    public decimal LimitAmount { get; private set; }

    private FinanceBudget() { }

    public FinanceBudget(Guid userId, DateOnly month, string category, decimal limitAmount)
    {
        UserId = userId;
        Month = month;
        Category = category;
        LimitAmount = limitAmount;
    }

    public void UpdateLimit(decimal limitAmount) => LimitAmount = limitAmount;
}
