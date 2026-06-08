using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.DTOs;

public class UpdateCategoryDto
{
    
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters.")]
    public string Name { get; set; } = string.Empty;
}