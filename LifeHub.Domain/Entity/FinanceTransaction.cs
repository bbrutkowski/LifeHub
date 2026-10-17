namespace LifeHub.Domain.Entity;

public class FinanceTransaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Category { get; private set; } = null!;
    public string? Note { get; private set; }
    public DateOnly Date { get; private set; }

    private FinanceTransaction() { }

    public FinanceTransaction(Guid userId, string type, decimal amount, string category, string? note, DateOnly date)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Amount = amount;
        Category = category;
        Note = note;
        Date = date;
    }
}
