namespace ExpenseTracker.Api.Models;

public class Transaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Expense";
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}

