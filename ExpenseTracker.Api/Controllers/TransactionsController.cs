using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;


[ApiController]
[Route("api/[controller]")]

public class TransactionsController : Controller
{

    private static readonly List<Transaction> Transactions =
    [
        new Transaction
        {
            Id = 1,
            Description = "Salary",
            Amount = 2500,
            Type = "Income",
            Category = "Job",
            Date = DateOnly.FromDateTime(DateTime.Now)
        },
        new Transaction
        {
            Id = 2,
            Description = "Groceries",
            Amount = 80,
            Type = "Expense",
            Category = "Food",
            Date = DateOnly.FromDateTime(DateTime.Today)
        }
    ];



    [HttpGet]
    public ActionResult<IEnumerable<Transaction>> GetAll()
    {
        return Ok(Transactions);
    }

    
    [HttpGet("{id:int}")]
    public ActionResult<Transaction> GetById(int id)
    {
        var transaction = Transactions.FirstOrDefault(x => x.Id == id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        return Ok(transaction);
    }

    [HttpPost]
    public ActionResult<Transaction> Create(Transaction transaction)
    {
        if (transaction.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }
        
        transaction.Id = Transactions.Count == 0 ? 1 : Transactions.Max(t => t.Id) + 1;
        
        Transactions.Add(transaction);
        
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }

    [HttpPut("{id:int}")]
    public ActionResult Update(int id, Transaction updatedTransaction)
    {
        var transaction = Transactions.FirstOrDefault(x => x.Id == id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        transaction.Description = updatedTransaction.Description;
        transaction.Amount = updatedTransaction.Amount;
        transaction.Type = updatedTransaction.Type;
        transaction.Category = updatedTransaction.Category;
        transaction.Date = updatedTransaction.Date;
        
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var transaction = Transactions.FirstOrDefault(x => x.Id == id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        Transactions.Remove(transaction);
        return NoContent();
    }
}