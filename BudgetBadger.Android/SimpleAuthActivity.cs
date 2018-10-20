using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using SimpleAuth.Droid.CustomTabs;

namespace BudgetBadger.Droid
{
	[Activity(NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
	[IntentFilter(new [] { Intent.ActionView}, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable}, DataScheme = "budgetbadger")]
    public class SimpleAuthActivity : SimpleAuthCallbackActivity
    {
    }
}
