using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;

    public TransactionService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<PagedResponseDto<TransactionResponseDto>> GetAllAsync(string? type, int? categoryId, DateOnly? fromDate, DateOnly? toDate, int pageNumber, int pageSize,
        string sortBy, string sortDirection)
    {
        var query = _context.Transactions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = NormalizeTransactionType(type);
            query = query.Where(t => t.Type == normalizedType);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.Date <= toDate.Value);
        }

        var normalizedSortDirection = sortDirection.ToLower();

        query = sortBy.ToLower() switch
        {
            "amount" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Amount)
                : query.OrderByDescending(t => t.Amount),
            
            "description" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Description)
                : query.OrderByDescending(t => t.Description),

            "type" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Type)
                : query.OrderByDescending(t => t.Type),

            "category" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Category!.Name)
                : query.OrderByDescending(t => t.Category!.Name),

            "date" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Date)
                : query.OrderByDescending(t => t.Date),

            _ => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Date)
                : query.OrderByDescending(t => t.Date)
        };

        var totalCount = await query.CountAsync();

        var transactions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty
            })
            .ToListAsync();

        return new PagedResponseDto<TransactionResponseDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Data = transactions
        };
        
        
    }

    public async Task<TransactionResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Where(t => t.Id == id)
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<TransactionResponseDto>> CreateAsync(CreateTransactionDto request)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return ServiceResult<TransactionResponseDto>.BadRequest("Category Does not Exists");
        }

        var transaction = new Transaction
        {
            Description = request.Description,
            Amount = request.Amount,
            Type = request.Type,
            Date = request.Date,
            CategoryId = request.CategoryId,
        };
        
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        var response = await GetByIdAsync(transaction.Id);
        
        return ServiceResult<TransactionResponseDto>.Ok(response!);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdateTransactionDto request)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
        {
            return ServiceResult<bool>.BadRequest("Transaction Does not Exists");
        }
        
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return ServiceResult<bool>.BadRequest("Category Does not Exists");
        }
        
        transaction.Description = request.Description.Trim();
        transaction.Amount = request.Amount;
        transaction.Type = NormalizeTransactionType(request.Type);
        transaction.Date = request.Date;
        transaction.CategoryId = request.CategoryId;
        
        await _context.SaveChangesAsync();
        
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
        {
            return ServiceResult<bool>.BadRequest("Transaction Does not Exists");
        }
        
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        
        return ServiceResult<bool>.Ok(true);
    }
    
    private static string NormalizeTransactionType(string type)
    {
        return type.Equals("Income", StringComparison.OrdinalIgnoreCase)
            ? "Income"
            : "Expense";
    }
}

