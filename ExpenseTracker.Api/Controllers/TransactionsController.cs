using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;


[ApiController]
[Route("api/[controller]")]

public class TransactionsController : Controller
{
    private readonly AppDbContext _context;

    public TransactionsController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<TransactionResponseDto>>> GetAll(
        string? type,
        int? categoryId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "date",
        string sortDirection = "desc"
        )
    {

        if (!string.IsNullOrWhiteSpace(type) && !IsValidTransactionType(type))
        {
            return BadRequest("Type must be either Income or Expense");
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            return BadRequest("From date must be greater than To date");
        }

        if (pageNumber < 1)
        {
            return BadRequest("Page number must be greater than 0");
        }

        if (pageSize < 1 || pageSize > 100)
        {   
            return BadRequest("Page size must be between 1 and 100");
        }

        var normalizedSortDirection = sortDirection.ToLower();

        if (normalizedSortDirection != "asc" && normalizedSortDirection != "desc")
        {
            return BadRequest("Sort direction must be either asc or desc");
        }

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

        query = sortBy.ToLower() switch
        {
            "amount" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Amount)
                : query.OrderByDescending(t => t.Amount),

            "description" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Description)
                : query.OrderByDescending((t => t.Description)),

            "type" => normalizedSortDirection == "asc"
                ? query.OrderBy(t => t.Type)
                : query.OrderByDescending(t => t.Type),

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
        }).ToListAsync();


        var response = new PagedResponseDto<TransactionResponseDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = transactions
        };
        
        return Ok(response);
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionResponseDto>> GetById(int id)
    {
        var transaction = await _context.Transactions.
            Where(x => x.Id == id).Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty
            }).FirstOrDefaultAsync();

        if (transaction == null)
        {
            return NotFound();
        }
        
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<CreateTransactionDto>> Create(CreateTransactionDto request)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist");
        }

        var transaction = new Transaction
        {
            Description = request.Description,
            Amount = request.Amount,
            Type = NormalizeTransactionType(request.Type),
            Date = request.Date,
            CategoryId = request.CategoryId,
        };
        
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var response = await _context.Transactions
            .Where(t => t.Id == transaction.Id)
            .Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : string.Empty
            }).FirstAsync();
            
        
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateTransactionDto request)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist");
        }
        
        transaction.Description = request.Description;
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Date = request.Date;
        transaction.CategoryId = request.CategoryId;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    private static bool IsValidTransactionType(string type)
    {
        return type.Equals("Income", StringComparison.OrdinalIgnoreCase)
               || type.Equals("Expense", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeTransactionType(string type)
    {
        return type.Equals("Income", StringComparison.OrdinalIgnoreCase)
            ? "Income"
            : "Expense";
    }
}