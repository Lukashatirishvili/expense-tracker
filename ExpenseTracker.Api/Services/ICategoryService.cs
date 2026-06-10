using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto?> GetByIdAsync(int id);
    Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto request);
    Task<ServiceResult<bool>> UpdateAsync(int id, UpdateCategoryDto request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}