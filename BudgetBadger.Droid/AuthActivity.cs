
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SimpleAuth.Droid.CustomTabs;

namespace BudgetBadger.Droid
{
    [Activity(NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
                  DataScheme = "budgetbadger")]
    public class AuthActivity : SimpleAuthCallbackActivity
    {
    }
}
