using ExpenseTracker.Api.Data;
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
    public async Task<ActionResult<IEnumerable<Transaction>>> GetAll()
    {

        var transactions = await _context.Transactions.ToListAsync();
        
        return Ok(transactions);
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Transaction>> GetById(int id)
    {
        var transaction = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> Create(Transaction transaction)
    {
        if (transaction.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }
        
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, Transaction updatedTransaction)
    {
        var transaction = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        transaction.Description = updatedTransaction.Description;
        transaction.Amount = updatedTransaction.Amount;
        transaction.Type = updatedTransaction.Type;
        transaction.Category = updatedTransaction.Category;
        transaction.Date = updatedTransaction.Date;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var transaction = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}