using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface ITransactionService
{
    Task<PagedResponseDto<TransactionResponseDto>> GetAllAsync(
        string? type,
        int? categoryId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection);
    
    Task<TransactionResponseDto?> GetByIdAsync(int id);
    Task<ServiceResult<TransactionResponseDto>> CreateAsync(CreateTransactionDto request);
    Task<ServiceResult<bool>> UpdateAsync(int id, UpdateTransactionDto request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
    
}