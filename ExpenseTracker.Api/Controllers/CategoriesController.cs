using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll()
    {
        var categories = await _context.Categories.OrderBy(c => c.Id).ToListAsync();
        
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }
        
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest("Category name is required");
        }

        var categoryExists = await _context.Categories.
            AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());

        if (categoryExists)
        {
            return BadRequest("Category with that name already exists");
        }
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, Category updatedCategory)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(updatedCategory.Name))
        {
            return BadRequest("Category name is required");
        }
        
        var categoryExists = await _context.Categories.
            AnyAsync(c => c.Id != id && c.Name.ToLower() == updatedCategory.Name.ToLower());

        if (categoryExists)
        {
            return BadRequest("Category Already exists");
        }
        
        category.Name = updatedCategory.Name;
        
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }
        
        var hasTransactions = await _context.Transactions.AnyAsync(t => t.CategoryId == id);

        if (hasTransactions)
        {
            return BadRequest("Cannot delete category because it has transactions");
        }
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}