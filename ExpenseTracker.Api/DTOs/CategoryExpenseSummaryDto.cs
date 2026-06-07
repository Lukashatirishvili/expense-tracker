namespace ExpenseTracker.Api.DTOs;

public class CategoryExpenseSummaryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Total  { get; set; }
}