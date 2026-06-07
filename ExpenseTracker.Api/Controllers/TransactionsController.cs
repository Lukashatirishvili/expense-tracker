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
    public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetAll()
    {

        var transactions = await _context.Transactions.Select(t => new TransactionResponseDto
        {
            Id = t.Id,
            Description = t.Description,
            Amount = t.Amount,
            Type = t.Type,
            Date = t.Date,
            CategoryId = t.CategoryId,
            CategoryName = t.Category != null ? t.Category.Name : string.Empty
        }).ToListAsync();
        
        return Ok(transactions);
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

        if (string.IsNullOrEmpty(request.Description))
        {
            return BadRequest("Description is required");
        }
        
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        if (!IsValidTransactionType(request.Type))
        {
            return BadRequest("Type must be either Income or Expense");
        }
        
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

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest("Description is required");
        }

        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        if (!IsValidTransactionType(request.Type))
        {
            return BadRequest("Type must be either Income or Expense");
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