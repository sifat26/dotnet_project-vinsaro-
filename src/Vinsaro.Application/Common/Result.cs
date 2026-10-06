namespace Vinsaro.Application.Common;

/// <summary>
/// Standard result wrapper used by all application services.
/// Avoids throwing exceptions for expected business failures.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }
    public int StatusCode { get; private init; }

    private Result() { }

    public static Result<T> Success(T value) =>
        new() { IsSuccess = true, Value = value, StatusCode = 200 };

    public static Result<T> Failure(string error, int statusCode = 400) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };

    public static Result<T> NotFound(string error = "Resource not found.") =>
        new() { IsSuccess = false, Error = error, StatusCode = 404 };

    public static Result<T> Forbidden(string error = "Access denied.") =>
        new() { IsSuccess = false, Error = error, StatusCode = 403 };

    public static Result<T> Unauthorized(string error = "Authentication required.") =>
        new() { IsSuccess = false, Error = error, StatusCode = 401 };
}

/// <summary>Non-generic result for operations that return no value.</summary>
public class Result
{
    public bool IsSuccess { get; private init; }
    public string? Error { get; private init; }
    public int StatusCode { get; private init; }

    private Result() { }

    public static Result Success() => new() { IsSuccess = true, StatusCode = 200 };
    public static Result Failure(string error, int statusCode = 400) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
    public static Result NotFound(string error = "Resource not found.") =>
        new() { IsSuccess = false, Error = error, StatusCode = 404 };
    public static Result Forbidden(string error = "Access denied.") =>
        new() { IsSuccess = false, Error = error, StatusCode = 403 };
}
