using System;

public sealed class OperationResult
{
    private const string DEFAULT_SUCCESS_MESSAGE = "";
    private const string DEFAULT_ERROR_MESSAGE = "An unexpected error occurred while processing the request.";

    private OperationResult(bool success, string errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public bool Success { get; }
    public string ErrorMessage { get; }

    public bool IsSuccess => Success;
    public string Message => ErrorMessage;

    public static OperationResult Ok()
    {
        return new OperationResult(success: true, errorMessage: DEFAULT_SUCCESS_MESSAGE);
    }

    public static OperationResult Ok(string message)
    {
        string safeMessage = (message ?? string.Empty).Trim();
        return new OperationResult(success: true, errorMessage: safeMessage);
    }

    public static OperationResult Fail(string message)
    {
        string safeMessage = (message ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(safeMessage))
        {
            safeMessage = DEFAULT_ERROR_MESSAGE;
        }

        return new OperationResult(success: false, errorMessage: safeMessage);
    }

    public static OperationResult Fail(Exception ex)
    {
        if (ex == null)
        {
            return Fail(DEFAULT_ERROR_MESSAGE);
        }

        return Fail(ex.Message);
    }
}