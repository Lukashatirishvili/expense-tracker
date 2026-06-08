using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
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
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)    
            .Select(t => new CategoryResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                TransactionCount = t.Transactions.Count
                
            })
            .ToListAsync();
        
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(int id)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                TransactionCount = c.Transactions.Count
                
            })
            .FirstOrDefaultAsync();
          

        if (category == null)
        {
            return NotFound();
        }
        
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create(CreateCategoryDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Category name is required");
        }

        var normalizedName = request.Name.Trim();

        var categoryExists = await _context.Categories.
            AnyAsync(c => c.Name.ToLower() == normalizedName.ToLower());

        var category = new Category
        {
            Name = request.Name
        };
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            TransactionCount = 0
        };
        
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateCategoryDto request)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }
        
        var normalizedName = request.Name.Trim();
        
        var categoryExists = await _context.Categories.
            AnyAsync(c => c.Id != id && c.Name.ToLower() == normalizedName.ToLower());

        if (categoryExists)
        {
            return BadRequest("Category Already exists");
        }
        
        category.Name = request.Name;
        
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