using System;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BudgetBadger.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(BorderlessPickerEffect), "BorderlessPickerEffect")]
namespace BudgetBadger.Droid.Effects
{
    public class BorderlessPickerEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is EditText picker)
            {
                picker.SetBackground(null);

                var layoutParams = new ViewGroup.MarginLayoutParams(Control.LayoutParameters);
                layoutParams.SetMargins(0, 0, 0, 0);
                picker.LayoutParameters = layoutParams;
                picker.SetPadding(0, 0, 0, 0);
                picker.ImeOptions = (ImeAction)ImeFlags.NoExtractUi;
            }

        }

        protected override void OnDetached()
        {
        }
    }
}

