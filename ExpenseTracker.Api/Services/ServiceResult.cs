namespace ExpenseTracker.Api.Services;

public class ServiceResult<T>
{
    public bool Success => ErrorType == ServiceErrorType.None;
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public ServiceErrorType ErrorType { get; set; } = ServiceErrorType.None;

    public static ServiceResult<T> Ok(T data)
    {
        return new ServiceResult<T>
        {
            Data = data,
            ErrorType = ServiceErrorType.None
        };
    }

    public static ServiceResult<T> BadRequest(string errorMessage)
    {
        return new ServiceResult<T>
        {
            ErrorMessage = errorMessage,
            ErrorType = ServiceErrorType.BadRequest
        };
    }

    public static ServiceResult<T> NotFound(string errorMessage)
    {
        return new ServiceResult<T>
        {
            ErrorMessage = errorMessage,
            ErrorType = ServiceErrorType.NotFound
        };
    }
}