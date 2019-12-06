using System;
using Prism.Logging;

namespace BudgetBadger.Forms
{
    public class ConsoleLogger : ILoggerFacade
    {
        public void Log(string message, Category category, Priority priority)
        {
            Console.WriteLine(message + Environment.NewLine + category + Environment.NewLine + priority + Environment.NewLine + Environment.NewLine);
        }
    }
}
