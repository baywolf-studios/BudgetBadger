using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.Utilities
{
    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this Result result)
        {
            var newResult = new Result<T> { Success = result.Success, Message = result.Message };

            return newResult;
        }
    }
}
