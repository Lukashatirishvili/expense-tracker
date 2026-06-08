using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.DTOs;

public class UpdateTransactionDto
{
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters.")]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Type is required.")]
    [RegularExpression(@"(?i)^(Income|Expense)$", ErrorMessage = "Type must be either Income or Expense.")]
    public string Type { get; set; } = "Expense";
    
    public DateOnly Date { get; set; } =  DateOnly.FromDateTime(DateTime.Today);
    
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than zero.")]
    public int CategoryId { get; set; }
}