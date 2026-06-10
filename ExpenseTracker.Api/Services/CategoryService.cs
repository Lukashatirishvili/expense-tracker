using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)    
            .Select(t => new CategoryResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                TransactionCount = t.Transactions.Count
                
            })
            .ToListAsync();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(int id)
    {
         return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                TransactionCount = c.Transactions.Count
                
            })
            .FirstOrDefaultAsync();
        
    }

    public async Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto request)
    {
        var normalizedName = request.Name.Trim();

        if (string.IsNullOrEmpty(normalizedName))
        {
            return ServiceResult<CategoryResponseDto>.BadRequest("Category name is required");
        }
        
        var categoryExists = await _context.Categories.AnyAsync(c => c.Name == normalizedName);

        if (categoryExists)
        {
            return ServiceResult<CategoryResponseDto>.BadRequest("Category with that name already exists");
        }

        var category = new Category
        {
            Name = normalizedName
        };
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            TransactionCount = 0
        };
        
        return ServiceResult<CategoryResponseDto>.Ok(response);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdateCategoryDto request)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return ServiceResult<bool>.NotFound("Category with that id does not exist");
        }
        
        var normalizedName = request.Name.Trim();

        if (string.IsNullOrEmpty(normalizedName))
        {
            return ServiceResult<bool>.BadRequest("Category name is required");
        }
        
        var categoryExists = await _context.Categories.AnyAsync(c => c.Name == normalizedName);

        if (categoryExists)
        {
            return ServiceResult<bool>.BadRequest("Category with that name already exists");
        }

        category.Name = normalizedName;
        
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return ServiceResult<bool>.BadRequest("Category name is required");
        }
        
        var hasTransaction = await _context.Transactions.AnyAsync(t => t.CategoryId == id);

        if (hasTransaction)
        {
            return ServiceResult<bool>.BadRequest("Cannot delete category because it has transactions");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }
}