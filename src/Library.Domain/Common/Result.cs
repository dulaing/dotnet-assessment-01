namespace Library.Domain.Common
{
    // explicit outcome model so expected failures (not_found, conflict, validation) are returned as data instead of thrown as exceptions
    public enum ResultErrorType
    {
        NotFound,
        Conflict,
        Validation
    }

    public record ResultError(ResultErrorType Type, string Code, string Message);

    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public ResultError? Error { get; }

        private Result(bool isSuccess, T? value, ResultError? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new(true, value, null);
  
        public static Result<T> Failure(ResultError error) => new(false, default, error);

        public static Result<T> NotFound(string code, string message) => Failure(new ResultError(ResultErrorType.NotFound, code, message));

        public static Result<T> Conflict(string code, string message) => Failure(new ResultError(ResultErrorType.Conflict, code, message));

        public static Result<T> Validation(string code, string message) => Failure(new ResultError(ResultErrorType.Validation, code, message));
    }
}