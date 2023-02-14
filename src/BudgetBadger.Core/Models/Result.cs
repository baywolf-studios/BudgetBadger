namespace BudgetBadger.Core.Models
{
    public class Result
    {
        public Result()
        {
        }

        protected Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public bool Failure => !Success;
        public string Message { get; set; }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default, false, message);
        }

        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }
    }

    public class Result<T> : Result
    {
        public Result()
        {
        }

        public Result(T data, bool success, string message) : base(success, message)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}