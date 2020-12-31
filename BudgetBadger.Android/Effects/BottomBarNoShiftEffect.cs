using System;
using Android.Views;
using BudgetBadger.Droid.Effects;
using Google.Android.Material.BottomNavigation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(BottomBarNoShiftEffect), "BottomBarNoShiftEffect")]
namespace BudgetBadger.Droid.Effects
{
    public class BottomBarNoShiftEffect : PlatformEffect
    {
        protected override void OnAttached ()
        {
            if (!(Container.GetChildAt(0) is ViewGroup layout))
                return;

            if (!(layout.GetChildAt(1) is BottomNavigationView bottomNavigationView))
                return;

            // This is what we set to adjust if the shifting happens
            bottomNavigationView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
        }

        protected override void OnDetached ()
        {
        }

    }
}
