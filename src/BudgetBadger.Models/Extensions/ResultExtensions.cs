using System;
namespace BudgetBadger.Models.Extensions
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
