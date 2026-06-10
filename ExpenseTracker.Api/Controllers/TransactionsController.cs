using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;


[ApiController]
[Route("api/[controller]")]

public class TransactionsController : Controller
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
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

        
        var response = await _transactionService.GetAllAsync(
            type,
            categoryId,
            fromDate,
            toDate,
            pageNumber,
            pageSize,
            sortBy,
            sortDirection);
        
        return Ok(response);
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionResponseDto>> GetById(int id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);

        if (transaction == null)
        {
            return NotFound();
        }
        
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<CreateTransactionDto>> Create(CreateTransactionDto request)
    {
        var result = await _transactionService.CreateAsync(request);

        if (!result.Success)
        {
            return HandleServiceError(result);
        }
            
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateTransactionDto request)
    {
        var result = await _transactionService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return HandleServiceError(result);
        }
        
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _transactionService.DeleteAsync(id);

        if (!result.Success)
        {
            return HandleServiceError(result);
        }
        
        return NoContent();
    }
    
    private ActionResult HandleServiceError<T>(ServiceResult<T> result)
    {
        return result.ErrorType switch
        {
            ServiceErrorType.BadRequest => BadRequest(result.ErrorMessage),
            ServiceErrorType.NotFound => NotFound(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage)
        };
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