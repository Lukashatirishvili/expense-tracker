namespace ExpenseTracker.Api.DTOs;

public class TransactionResponseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Expense";
    public DateOnly Date { get; set; } 
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}