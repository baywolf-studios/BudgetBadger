using System;
using Android.Text;
using BudgetBadger.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(CalculatorKeyboardEntryEffect), "CalculatorKeyboardEntryEffect")]
namespace BudgetBadger.Droid.Effects
{
    public class CalculatorKeyboardEntryEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is FormsEditText control)
            {
                //nothing right now
                //control.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
