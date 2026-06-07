namespace ExpenseTracker.Api.DTOs;

public class UpdateTransactionDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Expense";
    public DateOnly Date { get; set; } =  DateOnly.FromDateTime(DateTime.Today);
    public int CategoryId { get; set; }
}