namespace ExpenseTracker.Api.DTOs;

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
}