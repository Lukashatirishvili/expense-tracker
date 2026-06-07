namespace ExpenseTracker.Api.DTOs;

public class MonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance  { get; set; }
    public List<CategoryExpenseSummaryDto> ExpensesByCategory { get; set; } = [];
}