using System;
using System.Threading.Tasks;

namespace BudgetBadger.Core.Utilities
{
    public static class Async
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForget(this Task task, Action<Exception> errorHandler = null)
#pragma warning restore RECS0165
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex);
            }
        }
    }
}
