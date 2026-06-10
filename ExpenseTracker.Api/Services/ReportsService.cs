using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class ReportsService : IReportService
{
    private readonly AppDbContext _context;

    public ReportsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<MonthlySummaryDto>> GetMonthlySummaryAsync(int year, int month)
    {
        if (year < 2000 || year > 2100)
        {
            return ServiceResult<MonthlySummaryDto>.BadRequest("Year must be between 2000 and 2100.");
        }

        if (month < 1 || month > 12)
        {
            return ServiceResult<MonthlySummaryDto>.BadRequest("Month must be between 1 and 12.");
        }
        
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var monthlyTransactions = _context.Transactions
            .Where(t => t.Date >= startDate && t.Date < endDate);

        var totalIncome = await monthlyTransactions.
            Where(t => t.Type == "Income")
            .SumAsync(t => t.Amount);
        
        var totalExpense = await monthlyTransactions
            .Where(t => t.Type == "Expense")
            .SumAsync(t => t.Amount);
        
        var expensesByCategory = await monthlyTransactions
            .Where(t => t.Type == "Expense")
            .GroupBy(t => new
            {
                t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty,
            })
            .Select(g => new CategoryExpenseSummaryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName =  g.Key.CategoryName,
                Total =  g.Sum(t => t.Amount)
            })
            .OrderByDescending(t => t.Total)
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
        
        return ServiceResult<MonthlySummaryDto>.Ok(summary);
    }
}