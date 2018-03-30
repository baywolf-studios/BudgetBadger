using System;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BudgetBadger.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("Wolf")]
[assembly: ExportEffect(typeof(BorderlessEntryEffect), "BorderlessEntryEffect")]
namespace BudgetBadger.Droid.Effects
{
    public class BorderlessEntryEffect : PlatformEffect
    {
        public BorderlessEntryEffect()
        {
        }

        protected override void OnAttached()
        {
            if (Control is FormsEditText control)
            {
                control.SetBackground(null);

                var layoutParams = new ViewGroup.MarginLayoutParams(Control.LayoutParameters);
                layoutParams.SetMargins(0, 0, 0, 0);
                control.LayoutParameters = layoutParams;
                control.SetPadding(0, 0, 0, 0);
                control.ImeOptions = (ImeAction)ImeFlags.NoExtractUi;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
