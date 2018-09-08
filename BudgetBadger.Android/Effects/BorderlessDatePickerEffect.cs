using System;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BudgetBadger.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(BorderlessDatePickerEffect), "BorderlessDatePickerEffect")]
namespace BudgetBadger.Droid.Effects
{
    public class BorderlessDatePickerEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is EditText datePicker)
            {
                datePicker.SetBackground(null);

                var layoutParams = new ViewGroup.MarginLayoutParams(Control.LayoutParameters);
                layoutParams.SetMargins(0, 0, 0, 0);
                datePicker.LayoutParameters = layoutParams;
                datePicker.SetPadding(0, 0, 0, 0);
                datePicker.ImeOptions = (ImeAction)ImeFlags.NoExtractUi;
            }

        }

        protected override void OnDetached()
        {
        }
    }
}
