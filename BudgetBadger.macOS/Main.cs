using System;
using AppKit;

namespace BudgetBadger.macOS
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.SharedApplication.Delegate = new AppDelegate();
            try
            {
                NSApplication.Main(args);
            }
            catch (Exception ex)
            {
                var test = ex;
                throw;
            }
        }
    }
}
