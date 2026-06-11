namespace ExpenseTracker.Api.Services;

public interface ICurrentUserService
{
    string UserId { get; }
}