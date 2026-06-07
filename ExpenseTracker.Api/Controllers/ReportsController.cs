using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : Controller
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryDto>> GetMonthlySummary(int year, int month)
    {
        if (year < 2000 || year > 2100)
        {
            return BadRequest("Year must be between 2000 and 2100");
        }

        if (month < 1 || month > 12)
        {
            return BadRequest("Month must be between 1 to 12");
        }
        
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var monthlyTransactions = _context.Transactions
            .Where(t => t.Date >= startDate && t.Date < endDate);
        
        var totalIncome = await monthlyTransactions
            .Where(t => t.Type == "Income")
            .SumAsync(t => t.Amount);
        
        var totalExpense = await monthlyTransactions
            .Where(t => t.Type == "Expense")
            .SumAsync(t => t.Amount);

        var expensesByCategory = await monthlyTransactions
            .Where(t => t.Type == "Expense")
            .GroupBy(t => new
            {
                t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty
            })
            .Select(g => new CategoryExpenseSummaryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Total = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        var summary = new MonthlySummaryDto
        {
            Year = year,
            Month = month,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = totalIncome - totalExpense,
            ExpensesByCategory = expensesByCategory
        };
        
        return Ok(summary);
    }
}