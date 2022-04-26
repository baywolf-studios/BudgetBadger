using System;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Enums
{
    public static class Purchases
    {
        public static readonly string Pro = (Device.RuntimePlatform == Device.macOS ? "BudgetBadgerProMac" : "budgetbadgerpro");
    }
}
