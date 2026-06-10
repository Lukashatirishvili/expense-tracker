using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface IReportService
{
    Task<ServiceResult<MonthlySummaryDto>> GetMonthlySummaryAsync(int year, int month);
}